namespace HttpsRichardy.Federation.TestSuite.Integration.Fixtures;

public sealed class MongoDatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = default!;
    public string DatabaseName { get; private set; } = AppDomain.CurrentDomain.FriendlyName;

    public IMongoDatabase Database { get; private set; } = default!;
    public IMongoClient Client { get; private set; } = default!;

    // build a container with the official mongo image
    // see more: https://dotnet.testcontainers.org/
    private readonly IContainer _container = new ContainerBuilder("mongo:latest")
        .WithCleanUp(true)
        .WithExposedPort(27017)
        .WithPortBinding(0, 27017)
        .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
        .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "admin")
        .Build();

    public async Task DisposeAsync() => await _container.StopAsync();
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var hostPort = _container.GetMappedPublicPort(27017);
        var url = new MongoUrlBuilder
        {
            Scheme = ConnectionStringScheme.MongoDB,
            Server = new MongoServerAddress("localhost", hostPort),
            Username = "admin",
            Password = "admin",
            AuthenticationSource = "admin",
            DatabaseName = DatabaseName
        };

        // mongo database connection string format:
        // https://www.mongodb.com/docs/manual/reference/connection-string/
        ConnectionString = url.ToString();

        Client = new MongoClient(ConnectionString);
        Database = Client.GetDatabase(DatabaseName);
    }

    public async Task CleanDatabaseAsync()
    {
        var collections = await Database
            .ListCollectionNames()
            .ToListAsync();

        foreach (var collection in collections)
        {
            await Database.DropCollectionAsync(collection);
        }
    }
}
