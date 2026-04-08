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
        var client = _fixture.Build<Client>()
            .With(client => client.Name, "federation-admin")
            .With(client => client.IsDeleted, false)
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

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}