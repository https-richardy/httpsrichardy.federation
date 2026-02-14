namespace HttpsRichardy.Federation.TestSuite.Integration.Fixtures;

public sealed class IntegrationEnvironmentFixture : IAsyncLifetime
{
    private readonly MongoDatabaseFixture _databaseFixture;
    private readonly WebApplicationFixture _factory;

    public HttpClient HttpClient => _factory.HttpClient;
    public IMongoDatabase Database => _databaseFixture.Database;
    public IMongoClient Client => _databaseFixture.Client;
    public IServiceProvider Services => _factory.Services;

    public IntegrationEnvironmentFixture()
    {
        _databaseFixture = new MongoDatabaseFixture();
        _factory = new WebApplicationFixture();
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        await _databaseFixture.CleanDatabaseAsync();
        await _factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }
}

