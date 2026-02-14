namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class PermissionPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly IPermissionCollection _permissionCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Fixture _fixture = new();

    public PermissionPersistenceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;
        _permissionCollection = new PermissionCollection(_database, _realmProvider.Object);
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a permission, then it must persist in the database")]
    public async Task WhenInsertingAPermission_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create permission and matching filter */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permission = _fixture.Build<Permission>()
            .With(permission => permission.Name, "read:users")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create();

        var filters = PermissionFilters.WithSpecifications()
            .WithName(permission.Name)
            .Build();

        /* act: persist permission and query using name filter */
        await _permissionCollection.InsertAsync(permission);

        var result = await _permissionCollection.GetPermissionsAsync(filters, CancellationToken.None);
        var retrievedPermission = result.FirstOrDefault();

        /* assert: permission must be retrieved with same id and name */
        Assert.NotNull(retrievedPermission);

        Assert.Equal(permission.Id, retrievedPermission.Id);
        Assert.Equal(permission.Name, retrievedPermission.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a permission, then updated fields must persist")]
    public async Task WhenUpdatingAPermission_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert permission */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permission = _fixture.Build<Permission>()
            .With(permission => permission.Name, "update.test")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create();

        await _permissionCollection.InsertAsync(permission);

        /* act: update name and save */
        var newName = "updated.permission";

        permission.Name = newName;

        await _permissionCollection.UpdateAsync(permission);

        var filters = PermissionFilters.WithSpecifications()
            .WithName(newName)
            .Build();

        var result = await _permissionCollection.GetPermissionsAsync(filters, CancellationToken.None);
        var updatedPermission = result.FirstOrDefault();

        /* assert: updated permission must be found with new name */
        Assert.NotNull(updatedPermission);

        Assert.Equal(permission.Id, updatedPermission.Id);
        Assert.Equal(newName, updatedPermission.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a permission, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingAPermission_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert permission */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permission = _fixture.Build<Permission>()
            .With(permission => permission.Name, "delete.test")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create();

        await _permissionCollection.InsertAsync(permission);

        var filters = PermissionFilters.WithSpecifications()
            .WithName(permission.Name)
            .Build();

        /* act: delete permission and query by name */
        var deleted = await _permissionCollection.DeleteAsync(permission);

        var resultAfterDelete = await _permissionCollection.GetPermissionsAsync(filters, CancellationToken.None);

        /* assert: no permissions should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, permission => permission.Id == permission.Id);

        /* arrange: prepare filters including deleted permissions */
        var filtersWithDeleted = PermissionFilters.WithSpecifications()
            .WithName(permission.Name)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch permissions including deleted */
        var resultWithDeleted = await _permissionCollection.GetPermissionsAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: permission should be returned when including deleted permissions */
        Assert.Contains(resultWithDeleted, permission => permission.Id == permission.Id);

        Assert.True(permission.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering permissions, then it must return matching permissions")]
    public async Task WhenFilteringPermissions_ThenItMustReturnOnlyMatchingPermissions()
    {
        /* arrange: insert two permissions with different names */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permission1 = _fixture.Build<Permission>()
            .With(permission => permission.Name, "filter1")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create();

        var permission2 = _fixture.Build<Permission>()
            .With(permission => permission.Name, "filter2")
            .With(permission => permission.RealmId, realm.Id)
            .With(permission => permission.IsDeleted, false)
            .Create();

        await _permissionCollection.InsertAsync(permission1);
        await _permissionCollection.InsertAsync(permission2);

        var filters = PermissionFilters.WithSpecifications()
            .WithName("filter1")
            .Build();

        /* act: query permissions filtered by name */
        var filteredPermissions = await _permissionCollection.GetPermissionsAsync(filters, CancellationToken.None);

        /* assert: only permission1 should be returned */
        Assert.Single(filteredPermissions);
        Assert.Equal(permission1.Id, filteredPermissions.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 permissions with page size 5, then it must return 5 permissions per page")]
    public async Task WhenPaginatingTenPermissions_ThenItMustReturnFivePermissionsPerPage()
    {
        /* arrange: create and insert 10 permissions, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permissions = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Permission>()
            .With(permission => permission.Name, $"permission.{index}")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var permission in permissions)
        {
            await _permissionCollection.InsertAsync(permission);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = PermissionFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _permissionCollection.GetPermissionsAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 permissions */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = PermissionFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _permissionCollection.GetPermissionsAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 permissions */
        Assert.Equal(5, page2Results.Count);
    }

    [Fact(DisplayName = "[infrastructure] - when counting 10 permissions with isDeleted = false, then it must return 10")]
    public async Task WhenCountingTenPermissionsWithIsDeletedFalse_ThenItMustReturnTen()
    {
        /* arrange: create and insert 10 permissions, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var permissions = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Permission>()
            .With(permission => permission.Name, $"permission.{index}")
            .With(permission => permission.IsDeleted, false)
            .With(permission => permission.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var permission in permissions)
        {
            await _permissionCollection.InsertAsync(permission);
        }

        /* arrange: prepare filters with IsDeleted = false */
        var filters = PermissionFilters.WithSpecifications()
            .Build();

        /* act: count permissions matching filters */
        var total = await _permissionCollection.CountAsync(filters);

        /* assert: should return 10 */
        Assert.Equal(10, total);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
