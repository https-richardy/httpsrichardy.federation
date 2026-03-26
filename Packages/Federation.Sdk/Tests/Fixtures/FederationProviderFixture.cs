namespace HttpsRichardy.Federation.Sdk.TestSuite.Fixtures;

public sealed class FederationProviderFixture : IAsyncLifetime
{
    private IContainer _mongoContainer = default!;
    private IContainer _federationProviderContainer = default!;
    private INetwork _network = default!;

    public HttpClient HttpClient { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString())
            .Build();

        await _network.CreateAsync();

        _mongoContainer = new ContainerBuilder()
            .WithImage("mongo:latest")
            .WithNetwork(_network)
            .WithNetworkAliases("mongo")
            .WithCleanUp(true)
            .WithExposedPort(27017)
            .WithPortBinding(0, 27017)
            .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", "admin")
            .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", "admin")
            .Build();

        await _mongoContainer.StartAsync();

        var connectionString = $"mongodb://admin:admin@mongo:27017";

        _federationProviderContainer = new ContainerBuilder()
            .WithImage("httpsrichardy/federation:latest")
            .WithCleanUp(true)
            .WithNetwork(_network)
            .WithExposedPort(8080)
            .WithPortBinding(0, 8080)
            .WithEnvironment("Settings__Database__ConnectionString", connectionString)
            .WithEnvironment("Settings__Database__DatabaseName", "federation")
            .WithEnvironment("Settings__Administration__Username", "admin")
            .WithEnvironment("Settings__Administration__Password", "admin")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Application started"))
            .Build();

        await _federationProviderContainer.StartAsync();

        var port = _federationProviderContainer.GetMappedPublicPort(8080);
        var host = $"http://localhost:{port}";

        HttpClient = new HttpClient { BaseAddress = new Uri(host) };
    }

    public async Task DisposeAsync()
    {
        await _federationProviderContainer.StopAsync();
        await _federationProviderContainer.DisposeAsync();

        await _mongoContainer.StopAsync();
        await _mongoContainer.DisposeAsync();

        await _network.DeleteAsync();
        await _network.DisposeAsync();
    }
}
