namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class ClientPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly IClientCollection _clientCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Fixture _fixture = new();

    public ClientPersistenceTests(MongoDatabaseFixture mongoFixture)
    {
        _mongoFixture = mongoFixture;
        _database = mongoFixture.Database;

        _clientCollection = new ClientCollection(
            database: _database,
            realmProvider: _realmProvider.Object
        );
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a client, then it must persist in the database")]
    public async Task WhenInsertingAClient_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create client and matching filter */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client = _fixture.Build<Client>()
            .With(client => client.Name, "federation-admin")
            .With(client => client.ClientId, "federation-admin-id")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        var filters = ClientFilters.WithSpecifications()
            .WithName(client.Name)
            .Build();

        /* act: persist client and query using name filter */
        await _clientCollection.InsertAsync(client);

        var result = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);
        var retrievedClient = result.FirstOrDefault();

        /* assert: client must be retrieved with same id and name */
        Assert.NotNull(retrievedClient);

        Assert.Equal(client.Id, retrievedClient.Id);
        Assert.Equal(client.Name, retrievedClient.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a client, then updated fields must persist")]
    public async Task WhenUpdatingAClient_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert client */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client = _fixture.Build<Client>()
            .With(client => client.Name, "update.test")
            .With(client => client.ClientId, "update.test.client")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        await _clientCollection.InsertAsync(client);

        /* act: update name and save */
        var newName = "updated.client";

        client.Name = newName;

        await _clientCollection.UpdateAsync(client);

        var filters = ClientFilters.WithSpecifications()
            .WithName(newName)
            .Build();

        var result = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);
        var updatedClient = result.FirstOrDefault();

        /* assert: updated client must be found with new name */
        Assert.NotNull(updatedClient);

        Assert.Equal(client.Id, updatedClient.Id);
        Assert.Equal(newName, updatedClient.Name);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a client, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingAClient_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert client */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client = _fixture.Build<Client>()
            .With(client => client.Name, "delete.test")
            .With(client => client.ClientId, "delete.test.client")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        await _clientCollection.InsertAsync(client);

        var filters = ClientFilters.WithSpecifications()
            .WithName(client.Name)
            .Build();

        /* act: delete client and query by name */
        var deleted = await _clientCollection.DeleteAsync(client);

        var resultAfterDelete = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);

        /* assert: no clients should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, current => current.Id == client.Id);

        /* arrange: prepare filters including deleted clients */
        var filtersWithDeleted = ClientFilters.WithSpecifications()
            .WithName(client.Name)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch clients including deleted */
        var resultWithDeleted = await _clientCollection.GetClientsAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: client should be returned when including deleted clients */
        Assert.Contains(resultWithDeleted, current => current.Id == client.Id);

        Assert.True(client.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering clients by id, then it must return matching client")]
    public async Task WhenFilteringClientsById_ThenItMustReturnMatchingClient()
    {
        /* arrange: insert two clients */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client1 = _fixture.Build<Client>()
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        var client2 = _fixture.Build<Client>()
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        await _clientCollection.InsertAsync(client1);
        await _clientCollection.InsertAsync(client2);

        var filters = ClientFilters.WithSpecifications()
            .WithIdentifier(client1.Id)
            .Build();

        /* act: query clients filtered by id */
        var filteredClients = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);

        /* assert: only client1 should be returned */
        Assert.Single(filteredClients);
        Assert.Equal(client1.Id, filteredClients.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering clients by current realm, then it must return matching clients")]
    public async Task WhenFilteringClientsByCurrentRealm_ThenItMustReturnMatchingClients()
    {
        /* arrange: insert two clients with different realm ids */
        var realm = _fixture.Create<Realm>();
        var anotherRealm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client1 = _fixture.Build<Client>()
            .With(client => client.RealmId, realm.Id)
            .With(client => client.IsDeleted, false)
            .Create();

        var client2 = _fixture.Build<Client>()
            .With(client => client.RealmId, anotherRealm.Id)
            .With(client => client.IsDeleted, false)
            .Create();

        await _clientCollection.InsertAsync(client1);
        await _clientCollection.InsertAsync(client2);

        var filters = ClientFilters.WithSpecifications()
            .Build();

        /* act: query clients filtered by current realm */
        var filteredClients = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);

        /* assert: only client1 should be returned */
        Assert.Single(filteredClients);
        Assert.Equal(client1.Id, filteredClients.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering clients by name, then it must return matching clients")]
    public async Task WhenFilteringClientsByName_ThenItMustReturnMatchingClients()
    {
        /* arrange: insert two clients with different names */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client1 = _fixture.Build<Client>()
            .With(client => client.Name, "filter1")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        var client2 = _fixture.Build<Client>()
            .With(client => client.Name, "filter2")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        await _clientCollection.InsertAsync(client1);
        await _clientCollection.InsertAsync(client2);

        var filters = ClientFilters.WithSpecifications()
            .WithName("filter1")
            .Build();

        /* act: query clients filtered by name */
        var filteredClients = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);

        /* assert: only client1 should be returned */
        Assert.Single(filteredClients);
        Assert.Equal(client1.Id, filteredClients.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering clients by client id, then it must return matching clients")]
    public async Task WhenFilteringClientsByClientId_ThenItMustReturnMatchingClients()
    {
        /* arrange: insert two clients with different client ids */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var client1 = _fixture.Build<Client>()
            .With(client => client.ClientId, "client.filter.1")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        var client2 = _fixture.Build<Client>()
            .With(client => client.ClientId, "client.filter.2")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create();

        await _clientCollection.InsertAsync(client1);
        await _clientCollection.InsertAsync(client2);

        var filters = ClientFilters.WithSpecifications()
            .WithClientId("client.filter.1")
            .Build();

        /* act: query clients filtered by client id */
        var filteredClients = await _clientCollection.GetClientsAsync(filters, CancellationToken.None);

        /* assert: only client1 should be returned */
        Assert.Single(filteredClients);
        Assert.Equal(client1.Id, filteredClients.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 clients with page size 5, then it must return 5 clients per page")]
    public async Task WhenPaginatingTenClients_ThenItMustReturnFiveClientsPerPage()
    {
        /* arrange: create and insert 10 clients, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var clients = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Client>()
            .With(client => client.Name, $"client.{index}")
            .With(client => client.ClientId, $"client-id.{index}")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var client in clients)
        {
            await _clientCollection.InsertAsync(client);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = ClientFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _clientCollection.GetClientsAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 clients */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = ClientFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _clientCollection.GetClientsAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 clients */
        Assert.Equal(5, page2Results.Count);
    }

    [Fact(DisplayName = "[infrastructure] - when counting 10 clients with isDeleted = false, then it must return 10")]
    public async Task WhenCountingTenClientsWithIsDeletedFalse_ThenItMustReturnTen()
    {
        /* arrange: create and insert 10 clients, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var clients = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Client>()
            .With(client => client.Name, $"client.{index}")
            .With(client => client.ClientId, $"client-id.{index}")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create())
            .ToList();

        await _clientCollection.InsertManyAsync(clients);

        /* arrange: prepare filters with IsDeleted = false */
        var filters = ClientFilters.WithSpecifications()
            .WithIsDeleted(false)
            .Build();

        /* act: count clients matching filters */
        var total = await _clientCollection.CountClientsAsync(filters);

        /* assert: should return 10 */
        Assert.Equal(10, (int)total);
    }

    [Fact(DisplayName = "[infrastructure] - when counting 10 clients with isDeleted = true, then it must return 0")]
    public async Task WhenCountingTenClientsWithIsDeletedTrue_ThenItMustReturnZero()
    {
        /* arrange: create and insert 10 clients, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var clients = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Client>()
            .With(client => client.Name, $"client.{index}")
            .With(client => client.ClientId, $"client-id.{index}")
            .With(client => client.IsDeleted, false)
            .With(client => client.RealmId, realm.Id)
            .Create())
            .ToList();

        await _clientCollection.InsertManyAsync(clients);

        /* arrange: prepare filters with IsDeleted = true */
        var filters = ClientFilters.WithSpecifications()
            .WithIsDeleted(true)
            .Build();

        /* act: count clients matching filters */
        var total = await _clientCollection.CountClientsAsync(filters);

        /* assert: should return 0 */
        Assert.Equal(0, (int)total);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
