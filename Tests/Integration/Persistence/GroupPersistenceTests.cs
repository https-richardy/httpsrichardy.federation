namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class GroupPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly IGroupCollection _groupCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Fixture _fixture = new();

    public GroupPersistenceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;
        _groupCollection = new GroupCollection(_database, _realmProvider.Object);
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a group, then it must persist in the database")]
    public async Task WhenInsertingAGroup_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create group and matching filter */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group = _fixture.Build<Group>()
            .With(group => group.Name, "read:groups")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        var filters = GroupFilters.WithSpecifications()
            .WithName(group.Name)
            .Build();

        /* act: persist group and query using name filter */
        await _groupCollection.InsertAsync(group);

        var result = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);
        var retrievedGroup = result.FirstOrDefault();

        /* assert: group must be retrieved with same id and name */
        Assert.NotNull(retrievedGroup);

        Assert.Equal(group.Id, retrievedGroup.Id);
        Assert.Equal(group.Name, retrievedGroup.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a group, then updated fields must persist")]
    public async Task WhenUpdatingAGroup_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert group */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group = _fixture.Build<Group>()
            .With(group => group.Name, "update.test")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        await _groupCollection.InsertAsync(group);

        /* act: update name and save */
        var newName = "updated.group";

        group.Name = newName;

        await _groupCollection.UpdateAsync(group);

        var filters = GroupFilters.WithSpecifications()
            .WithName(newName)
            .Build();

        var result = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);
        var updatedGroup = result.FirstOrDefault();

        /* assert: updated group must be found with new name */
        Assert.NotNull(updatedGroup);

        Assert.Equal(group.Id, updatedGroup.Id);
        Assert.Equal(newName, updatedGroup.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a group, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingAGroup_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert group */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group = _fixture.Build<Group>()
            .With(group => group.Name, "delete.test")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        await _groupCollection.InsertAsync(group);

        var filters = GroupFilters.WithSpecifications()
            .WithName(group.Name)
            .Build();

        /* act: delete group and query by name */
        var deleted = await _groupCollection.DeleteAsync(group);

        var resultAfterDelete = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);

        /* assert: no groups should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, g => g.Id == group.Id);

        /* arrange: prepare filters including deleted groups */
        var filtersWithDeleted = GroupFilters.WithSpecifications()
            .WithName(group.Name)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch groups including deleted */
        var resultWithDeleted = await _groupCollection.GetGroupsAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: group should be returned when including deleted groups */
        Assert.Contains(resultWithDeleted, g => g.Id == group.Id);

        Assert.True(group.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering groups by id, then it must return matching group")]
    public async Task WhenFilteringGroupsById_ThenItMustReturnMatchingGroup()
    {
        /* arrange: insert two groups */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group1 = _fixture.Build<Group>()
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        var group2 = _fixture.Build<Group>()
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        await _groupCollection.InsertAsync(group1);
        await _groupCollection.InsertAsync(group2);

        var filters = GroupFilters.WithSpecifications()
            .WithIdentifier(group1.Id)
            .Build();

        /* act: query groups filtered by id */
        var filteredGroups = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);

        /* assert: only group1 should be returned */
        Assert.Single(filteredGroups);
        Assert.Equal(group1.Id, filteredGroups.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering groups by realm id, then it must return matching groups")]
    public async Task WhenFilteringGroupsByRealmId_ThenItMustReturnMatchingGroups()
    {
        /* arrange: insert two groups with different realm ids */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group1 = _fixture.Build<Group>()
            .With(group => group.RealmId, realm.Id)
            .With(group => group.IsDeleted, false)
            .Create();

        var group2 = _fixture.Build<Group>()
            .With(group => group.IsDeleted, false)
            .Create();

        await _groupCollection.InsertAsync(group1);
        await _groupCollection.InsertAsync(group2);

        var filters = GroupFilters.WithSpecifications()
            .WithRealmId(realm.Id)
            .Build();

        /* act: query groups filtered by realm id */
        var filteredGroups = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);

        /* assert: only group1 should be returned */
        Assert.Single(filteredGroups);
        Assert.Equal(group1.Id, filteredGroups.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering groups by name, then it must return matching groups")]
    public async Task WhenFilteringGroupsByName_ThenItMustReturnMatchingGroups()
    {
        /* arrange: insert two groups with different names */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var group1 = _fixture.Build<Group>()
            .With(group => group.Name, "filter1")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        var group2 = _fixture.Build<Group>()
            .With(group => group.Name, "filter2")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create();

        await _groupCollection.InsertAsync(group1);
        await _groupCollection.InsertAsync(group2);

        var filters = GroupFilters.WithSpecifications()
            .WithName("filter1")
            .Build();

        /* act: query groups filtered by name */
        var filteredGroups = await _groupCollection.GetGroupsAsync(filters, CancellationToken.None);

        /* assert: only group1 should be returned */
        Assert.Single(filteredGroups);
        Assert.Equal(group1.Id, filteredGroups.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 groups with page size 5, then it must return 5 groups per page")]
    public async Task WhenPaginatingTenGroups_ThenItMustReturnFiveGroupsPerPage()
    {
        /* arrange: create and insert 10 groups, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var groups = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Group>()
            .With(group => group.Name, $"group.{index}")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var group in groups)
        {
            await _groupCollection.InsertAsync(group);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = GroupFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _groupCollection.GetGroupsAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 groups */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = GroupFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _groupCollection.GetGroupsAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 groups */
        Assert.Equal(5, page2Results.Count);
    }

    [Fact(DisplayName = "[infrastructure] - when counting 10 groups with isDeleted = false, then it must return 10")]
    public async Task WhenCountingTenGroupsWithIsDeletedFalse_ThenItMustReturnTen()
    {
        /* arrange: create and insert 10 groups, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var groups = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Group>()
            .With(group => group.Name, $"group.{index}")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var group in groups)
        {
            await _groupCollection.InsertAsync(group);
        }

        /* arrange: prepare filters with IsDeleted = false */
        var filters = GroupFilters.WithSpecifications()
            .WithIsDeleted(false)
            .Build();

        /* act: count groups matching filters */
        var total = await _groupCollection.CountAsync(filters);

        /* assert: should return 10 */
        Assert.Equal(10, total);
    }

    [Fact(DisplayName = "[infrastructure] - when counting 10 groups with isDeleted = true, then it must return 0")]
    public async Task WhenCountingTenGroupsWithIsDeletedTrue_ThenItMustReturnZero()
    {
        /* arrange: create and insert 10 groups, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var groups = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Group>()
            .With(group => group.Name, $"group.{index}")
            .With(group => group.IsDeleted, false)
            .With(group => group.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var group in groups)
        {
            await _groupCollection.InsertAsync(group);
        }

        /* arrange: prepare filters with IsDeleted = true */
        var filters = GroupFilters.WithSpecifications()
            .WithIsDeleted(true)
            .Build();

        /* act: count groups matching filters */
        var total = await _groupCollection.CountAsync(filters);

        /* assert: should return 0 */
        Assert.Equal(0, total);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
