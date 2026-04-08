namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class RealmPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly IRealmCollection _realmCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Fixture _fixture = new();

    public RealmPersistenceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;
        _realmCollection = new RealmCollection(_database);
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a realm, then it must persist in the database")]
    public async Task WhenInsertingARealm_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create realm and matching filter */
        var realm = _fixture.Build<Realm>()
            .With(realm => realm.Name, "HttpsRichardy")
            .With(realm => realm.IsDeleted, false)
            .Create();

        var filters = RealmFilters.WithSpecifications()
            .WithName(realm.Name)
            .Build();

        /* act: persist realm and query using name filter */
        await _realmCollection.InsertAsync(realm);

        var result = await _realmCollection.GetRealmsAsync(filters, CancellationToken.None);
        var retrievedRealm = result.FirstOrDefault();

        /* assert: realm must be retrieved with same id and name */
        Assert.NotNull(retrievedRealm);
        Assert.Equal(realm.Id, retrievedRealm.Id);
        Assert.Equal(realm.Name, retrievedRealm.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a realm, then updated fields must persist")]
    public async Task WhenUpdatingARealm_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert realm */
        var realm = _fixture.Build<Realm>()
            .With(realm => realm.Name, "update.test")
            .With(realm => realm.IsDeleted, false)
            .Create();

        await _realmCollection.InsertAsync(realm);

        /* act: update name and save */
        var newName = "updated.name";

        realm.Name = newName;

        await _realmCollection.UpdateAsync(realm);

        var filters = RealmFilters.WithSpecifications()
            .WithName(newName)
            .Build();

        var result = await _realmCollection.GetRealmsAsync(filters, CancellationToken.None);
        var updatedRealm = result.FirstOrDefault();

        /* assert: updated realm must be found with new name */
        Assert.NotNull(updatedRealm);

        Assert.Equal(realm.Id, updatedRealm.Id);
        Assert.Equal(newName, updatedRealm.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a realm, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingARealm_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert realm */
        var realm = _fixture.Build<Realm>()
            .With(realm => realm.Name, "delete.test")
            .With(realm => realm.IsDeleted, false)
            .Create();

        await _realmCollection.InsertAsync(realm);

        var filters = RealmFilters.WithSpecifications()
            .WithName(realm.Name)
            .Build();

        /* act: delete realm and query by name */
        var deleted = await _realmCollection.DeleteAsync(realm);

        var resultAfterDelete = await _realmCollection.GetRealmsAsync(filters, CancellationToken.None);

        /* assert: no realms should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, t => t.Id == realm.Id);

        /* arrange: prepare filters including deleted realms */
        var filtersWithDeleted = RealmFilters.WithSpecifications()
            .WithName(realm.Name)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch realms including deleted */
        var resultWithDeleted = await _realmCollection.GetRealmsAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: realm should be returned when including deleted realms */
        Assert.Contains(resultWithDeleted, t => t.Id == realm.Id);

        Assert.True(realm.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering realms, then it must return matching realms")]
    public async Task WhenFilteringRealms_ThenItMustReturnOnlyMatchingRealms()
    {
        /* arrange: insert two realms with different names */
        var realm1 = _fixture.Build<Realm>()
            .With(realm => realm.Name, "filter1")
            .With(realm => realm.IsDeleted, false)
            .Create();

        var realm2 = _fixture.Build<Realm>()
            .With(realm => realm.Name, "filter2")
            .With(realm => realm.IsDeleted, false)
            .Create();

        await _realmCollection.InsertAsync(realm1);
        await _realmCollection.InsertAsync(realm2);

        var filters = RealmFilters.WithSpecifications()
            .WithName("filter1")
            .Build();

        /* act: query realms filtered by name */
        var filteredRealms = await _realmCollection.GetRealmsAsync(filters, CancellationToken.None);

        /* assert: only realm1 should be returned */
        Assert.Single(filteredRealms);
        Assert.Equal(realm1.Id, filteredRealms.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 realms with page size 5, then it must return 5 realms per page")]
    public async Task WhenPaginatingTenRealms_ThenItMustReturnFiveRealmsPerPage()
    {
        /* arrange: create and insert 10 realms, all not deleted */
        var realms = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Realm>()
            .With(realm => realm.Name, $"realm.{index}")
            .With(realm => realm.IsDeleted, false)
            .Create())
            .ToList();

        foreach (var realm in realms)
        {
            await _realmCollection.InsertAsync(realm);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = RealmFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _realmCollection.GetRealmsAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 realms */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = RealmFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _realmCollection.GetRealmsAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 realms */
        Assert.Equal(5, page2Results.Count);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
