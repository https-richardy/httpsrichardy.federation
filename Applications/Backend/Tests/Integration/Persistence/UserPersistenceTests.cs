namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class UserPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly IUserCollection _userCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Fixture _fixture = new();

    public UserPersistenceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;
        _userCollection = new UserCollection(_database, _realmProvider.Object);
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a user, then it must persist in the database")]
    public async Task WhenInsertingAUser_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create user and matching filter */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var user = _fixture.Build<User>()
            .With(user => user.Username, "richard.garcia@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        var filters = UserFilters.WithSpecifications()
            .WithUsername(user.Username)
            .Build();

        /* act: persist user and query using username filter */
        await _userCollection.InsertAsync(user);

        var result = await _userCollection.GetUsersAsync(filters, CancellationToken.None);
        var retrievedUser = result.FirstOrDefault();

        /* assert: user must be retrieved with same id and username */
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Username, retrievedUser.Username);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a user, then updated fields must persist")]
    public async Task WhenUpdatingAUser_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert user */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var user = _fixture.Build<User>()
            .With(user => user.Username, "update.test@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        await _userCollection.InsertAsync(user);

        /* act: update username and save */
        var newUsername = "updated.email@coding.com";

        user.Username = newUsername;

        await _userCollection.UpdateAsync(user);

        var filters = UserFilters.WithSpecifications()
            .WithUsername(newUsername)
            .Build();

        var result = await _userCollection.GetUsersAsync(filters, CancellationToken.None);
        var updatedUser = result.FirstOrDefault();

        /* assert: updated user must be found with new username */
        Assert.NotNull(updatedUser);

        Assert.Equal(user.Id, updatedUser.Id);
        Assert.Equal(newUsername, updatedUser.Username);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a user, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingAUser_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert user */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var user = _fixture.Build<User>()
            .With(user => user.Username, "delete.test@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        await _userCollection.InsertAsync(user);

        var filters = UserFilters.WithSpecifications()
            .WithUsername(user.Username)
            .Build();

        /* act: delete user and query by username */
        var deleted = await _userCollection.DeleteAsync(user);

        var resultAfterDelete = await _userCollection.GetUsersAsync(filters, CancellationToken.None);

        /* assert: no users should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, user => user.Id == user.Id);

        /* arrange: prepare filters including deleted users */
        var filtersWithDeleted = UserFilters.WithSpecifications()
            .WithUsername(user.Username)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch users including deleted */
        var resultWithDeleted = await _userCollection.GetUsersAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: user should be returned when including deleted users */
        Assert.Contains(resultWithDeleted, user => user.Id == user.Id);

        Assert.True(user.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering users, then it must return matching users")]
    public async Task WhenFilteringUsers_ThenItMustReturnOnlyMatchingUsers()
    {
        /* arrange: insert two users with different usernamess */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var user1 = _fixture.Build<User>()
            .With(user => user.Username, "filter1@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        var user2 = _fixture.Build<User>()
            .With(user => user.Username, "filter2@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        await _userCollection.InsertAsync(user1);
        await _userCollection.InsertAsync(user2);

        var filters = UserFilters.WithSpecifications()
            .WithUsername("filter1@coding.com")
            .Build();

        /* act: query users filtered by username */
        var filteredUsers = await _userCollection.GetUsersAsync(filters, CancellationToken.None);

        /* assert: only user1 should be returned */
        Assert.Single(filteredUsers);
        Assert.Equal(user1.Id, filteredUsers.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 users with page size 5, then it must return 5 users per page")]
    public async Task WhenPaginatingTenUsers_ThenItMustReturnFiveUsersPerPage()
    {
        /* arrange: create and insert 10 users, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var users = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<User>()
            .With(user => user.Username, $"user.{index}@coding.com")
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var user in users)
        {
            await _userCollection.InsertAsync(user);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = UserFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _userCollection.GetUsersAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 users */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = UserFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _userCollection.GetUsersAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 users */
        Assert.Equal(5, page2Results.Count);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering users by realm id, then only users from that realm are returned")]
    public async Task WhenFilteringUsersByRealmId_ThenOnlyUsersFromThatRealmAreReturned()
    {
        /* arrange: create realm and user with realm id */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var user = _fixture.Build<User>()
            .With(user => user.Username, "richard.garcia@coding.com")
            .With(user => user.RealmId, realm.Id)
            .With(user => user.IsDeleted, false)
            .Create();

        await _userCollection.InsertAsync(user);

        var filters = UserFilters.WithSpecifications()
            .WithRealmId(realm.Id)
            .WithUsername(user.Username)
            .Build();

        /* act: query users filtered by realm id and username */
        var result = await _userCollection.GetUsersAsync(filters, CancellationToken.None);
        var retrievedUser = result.FirstOrDefault();

        /* assert: only user with matching realm id is returned */
        Assert.NotNull(retrievedUser);

        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Username, retrievedUser.Username);
        Assert.Equal(realm.Id, retrievedUser.RealmId);
    }

    [Fact(DisplayName = "[infrastructure] - when counting users with filters, then count must reflect filtered records")]
    public async Task WhenCountingUsersWithFilters_ThenCountMustReflectFilteredRecords()
    {
        /* arrange: create 10 users with varied realmId, username and IsDeleted */
        var realm1 = _fixture.Create<Realm>();
        var realm2 = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm1);

        var users = new List<User>();

        for (int index = 0; index < 10; index++)
        {
            var user = _fixture.Build<User>()
                .With(user => user.RealmId, index < 5 ? realm1.Id : realm2.Id) // first 5 realm1, last 5 realm2
                .With(user => user.Username, $"user{index}@example.com")
                .With(user => user.IsDeleted, index % 3 == 0) // every third user is deleted
                .Create();

            users.Add(user);

            await _userCollection.InsertAsync(user);
        }

        /* act: count users filtered by realm1 and IsDeleted = false */
        var filters = UserFilters.WithSpecifications()
            .WithRealmId(realm1.Id)
            .WithIsDeleted(false)
            .Build();

        var filteredCount = await _userCollection.CountAsync(filters);
        var expectedCount = users.Count(user => user.RealmId == realm1.Id && !user.IsDeleted);

        /* assert: expected count of users for realm*/
        Assert.Equal(expectedCount, filteredCount);
    }

    #pragma warning disable S2325

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
