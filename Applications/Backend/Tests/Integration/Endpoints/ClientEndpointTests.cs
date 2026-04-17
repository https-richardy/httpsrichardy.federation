namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class ClientEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when GET /clients should return paginated list of clients")]
    public async Task WhenGetClients_ShouldReturnPaginatedListOfClients()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var realmCollection = factory.Services.GetRequiredService<IRealmCollection>();

        /* arrange: authenticate user and get access token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authenticationResult);
        Assert.NotEmpty(authenticationResult.AccessToken);

        httpClient.WithAuthorization(authenticationResult.AccessToken);

        /* arrange: insert 5 clients directly into the database */
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, CancellationToken.None);
        var realm = realms.FirstOrDefault();

        Assert.NotNull(realm);

        var clients = Enumerable.Range(1, 5)
            .Select(index => _fixture.Build<Client>()
            .With(client => client.Name, $"test-client-{index}")
            .With(client => client.ClientId, $"test-client-id-{index}")
            .With(client => client.Secret, $"test-client-secret-{index}")
            .With(client => client.RealmId, realm.Id)
            .With(client => client.IsDeleted, false)
            .Create())
            .ToList();

        await clientCollection.InsertManyAsync(clients);

        /* act: send GET request to retrieve clients */
        var response = await httpClient.GetAsync("api/v1/clients");
        var result = await response.Content.ReadFromJsonAsync<Pagination<ClientScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(result);
        Assert.NotNull(result.Items);

        /* assert: inserted clients should be present in the returned list */
        foreach (var client in clients)
        {
            Assert.Contains(result.Items, item => item.Id == client.Id);
        }
    }

    [Fact(DisplayName = "[e2e] - when POST /clients with valid data should create client successfully")]
    public async Task WhenPostClientsWithValidData_ShouldCreateClientSuccessfully()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();

        /* arrange: authenticate user and get access token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authenticationResult);
        Assert.NotEmpty(authenticationResult.AccessToken);

        httpClient.WithAuthorization(authenticationResult.AccessToken);

        /* arrange: prepare request to create a new client */
        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        /* act: send POST request to create client */
        var response = await httpClient.PostAsJsonAsync("api/v1/clients", payload);

        /* assert: response should be 201 Created */
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        /* assert: client must be persisted in the repository */
        var filters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, CancellationToken.None);
        var createdClient = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(createdClient);

        Assert.Equal(payload.Name, createdClient.Name);
        Assert.Equal(payload.Flows, createdClient.Flows);
    }

    [Fact(DisplayName = "[e2e] - when PUT /clients/{id} with valid data should update client successfully")]
    public async Task WhenPutClientsWithValidData_ShouldUpdateClientSuccessfully()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();

        /* arrange: authenticate user and get access token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authenticationResult);
        Assert.NotEmpty(authenticationResult.AccessToken);

        httpClient.WithAuthorization(authenticationResult.AccessToken);

        /* arrange: create a new client */
        var createPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/clients", createPayload);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var filters = ClientFilters.WithSpecifications()
            .WithName(createPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: prepare request to update client */
        var payload = _fixture.Build<ClientUpdateScheme>()
            .With(client => client.Name, $"updated-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.AuthorizationCode])
            .With(client => client.RedirectUris, ["https://localhost/callback"])
            .Create();

        /* act: send PUT request to update client */
        var response = await httpClient.PutAsJsonAsync($"api/v1/clients/{client.Id}", payload);
        var result = await response.Content.ReadFromJsonAsync<ClientScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);

        /* assert: client must be updated in the repository */
        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var matchingClients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var persistedClient = matchingClients.FirstOrDefault(current => current.Id == client.Id);

        Assert.NotEmpty(matchingClients);
        Assert.NotNull(persistedClient);

        Assert.Equal(client.Id, result.Id);
        Assert.Equal(payload.Name, result.Name);
        Assert.Equal(payload.Name, persistedClient.Name);
        Assert.Equal(payload.Flows, persistedClient.Flows);

        Assert.Contains(persistedClient.RedirectUris, uri => uri.Address == "https://localhost/callback");
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id} with valid client should delete client successfully")]
    public async Task WhenDeleteClientsWithValidClient_ShouldDeleteClientSuccessfully()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();

        /* arrange: authenticate user and get access token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authenticationResult);
        Assert.NotEmpty(authenticationResult.AccessToken);

        httpClient.WithAuthorization(authenticationResult.AccessToken);

        /* arrange: create a new client to delete */
        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/clients", payload);
        var filters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var clients = await clientCollection.GetClientsAsync(filters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* act: send DELETE request to remove client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}");
        var result = await clientCollection.GetClientsAsync(filters, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.DoesNotContain(result, current => current.Id == client.Id);
    }
}
