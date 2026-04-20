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

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id} with non-existent client should return 404 #ERROR-2D943")]
    public async Task WhenDeleteClientsWithNonExistentClient_ShouldReturnNotFound()
    {
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

        /* arrange: prepare request with a non-existent client ID */
        var nonExistentClientId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{nonExistentClientId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ClientErrors.ClientDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when GET /clients/{id}/permissions should return client's assigned permissions")]
    public async Task WhenGetClientPermissions_ShouldReturnAssignedPermissions()
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

        /* arrange: create and insert client with assigned permissions */
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, CancellationToken.None);
        var realm = realms.FirstOrDefault();

        Assert.NotNull(realm);

        var permission1 = _fixture.Build<Permission>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .With(permission => permission.RealmId, realm.Id)
            .With(permission => permission.IsDeleted, false)
            .Create();

        var permission2 = _fixture.Build<Permission>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .With(permission => permission.RealmId, realm.Id)
            .With(permission => permission.IsDeleted, false)
            .Create();

        var client = _fixture.Build<Client>()
            .With(current => current.Name, $"test-client-{Guid.NewGuid()}")
            .With(current => current.ClientId, $"test-client-id-{Guid.NewGuid()}")
            .With(current => current.Secret, $"test-client-secret-{Guid.NewGuid()}")
            .With(current => current.RealmId, realm.Id)
            .With(current => current.IsDeleted, false)
            .With(current => current.Permissions, [permission1, permission2])
            .Create();

        await clientCollection.InsertAsync(client);

        /* act: send GET request to retrieve client's permissions */
        var response = await httpClient.GetAsync($"api/v1/clients/{client.Id}/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);

        /* assert: assigned permissions should be returned */
        Assert.Contains(permissions, permission => permission.Name == permission1.Name);
        Assert.Contains(permissions, permission => permission.Name == permission2.Name);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/permissions with valid permission should assign permission successfully")]
    public async Task WhenPostClientPermissionsWithValidPermission_ShouldAssignPermissionSuccessfully()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var permissionCollection = factory.Services.GetRequiredService<IPermissionCollection>();

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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);

        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(permissionPayload.Name)
            .Build();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, CancellationToken.None);
        var permission = permissions.FirstOrDefault();

        Assert.NotEmpty(permissions);
        Assert.NotNull(permission);

        /* arrange: prepare request to assign permission to client */
        var payload = _fixture.Build<AssignClientPermissionScheme>()
            .With(assignment => assignment.PermissionName, permission.Name)
            .Create();

        /* act: send POST request to assign permission to client */
        var response = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/permissions", payload);
        var assignedPermissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK and permissions list should be returned */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(assignedPermissions);

        /* assert: the assigned permission should be in the returned list */
        Assert.Contains(assignedPermissions, current => current.Name == permission.Name);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/permissions with duplicate permission should return 409 #ERROR-8D71B")]
    public async Task WhenPostClientPermissionsWithDuplicatePermission_ShouldReturnConflict()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var permissionCollection = factory.Services.GetRequiredService<IPermissionCollection>();

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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);

        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(permissionPayload.Name)
            .Build();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, CancellationToken.None);
        var permission = permissions.FirstOrDefault();

        Assert.NotEmpty(permissions);
        Assert.NotNull(permission);

        /* arrange: assign permission to client first time */
        var payload = _fixture.Build<AssignClientPermissionScheme>()
            .With(assignment => assignment.PermissionName, permission.Name)
            .Create();

        var firstResponse = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/permissions", payload);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        /* act: attempt to assign the same permission again */
        var secondResponse = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/permissions", payload);
        var error = await secondResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal(ClientErrors.ClientAlreadyHasPermission, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/permissions/{permissionId} should revoke permission successfully")]
    public async Task WhenDeleteClientPermission_ShouldRevokePermissionSuccessfully()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var permissionCollection = factory.Services.GetRequiredService<IPermissionCollection>();

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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);

        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(permissionPayload.Name)
            .Build();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, CancellationToken.None);
        var permission = permissions.FirstOrDefault();

        Assert.NotEmpty(permissions);
        Assert.NotNull(permission);

        /* arrange: assign permission to client */
        var assignPayload = _fixture.Build<AssignClientPermissionScheme>()
            .With(assignment => assignment.PermissionName, permission.Name)
            .Create();

        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/permissions", assignPayload);

        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        /* act: send DELETE request to revoke permission from client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}/permissions/{permission.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        /* assert: verify permission is no longer in client's permissions list */
        var httpResponse = await httpClient.GetAsync($"api/v1/clients/{client.Id}/permissions");
        var assignedPermissions = await httpResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        Assert.NotNull(assignedPermissions);
        Assert.DoesNotContain(assignedPermissions, current => current.Id == permission.Id);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/permissions/{permissionId} with non-existent client should return 404 #ERROR-2D943")]
    public async Task WhenDeleteClientPermissionWithNonExistentClient_ShouldReturnNotFound()
    {
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

        /* arrange: prepare request with a non-existent client ID */
        var nonExistentClientId = Guid.NewGuid().ToString();
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{nonExistentClientId}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ClientErrors.ClientDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/permissions/{permissionId} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenDeleteClientPermissionWithNonExistentPermission_ShouldReturnNotFound()
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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request with non-existent permission */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/permissions/{permissionId} with permission not assigned should return 409 #ERROR-C2FB0")]
    public async Task WhenDeleteClientPermissionWithPermissionNotAssigned_ShouldReturnConflict()
    {
        /* arrange: resolve required dependencies */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var permissionCollection = factory.Services.GetRequiredService<IPermissionCollection>();

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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: create a new permission without assigning it to the client */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(permissionPayload.Name)
            .Build();

        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, CancellationToken.None);
        var permission = permissions.FirstOrDefault();

        Assert.NotEmpty(permissions);
        Assert.NotNull(permission);

        /* act: send DELETE request for permission not assigned to client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}/permissions/{permission.Id}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ClientErrors.PermissionNotAssigned, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/audiences with valid audience should assign audience successfully")]
    public async Task WhenPostClientAudiencesWithValidAudience_ShouldAssignAudienceSuccessfully()
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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: prepare request to assign audience to client */
        var payload = new AssignClientAudienceScheme
        {
            Value = "https://api.example.com"
        };

        /* act: send POST request to assign audience to client */
        var response = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", payload);
        var audiences = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>();

        /* assert: response should be 200 OK and audiences list should be returned */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(audiences);

        /* assert: the assigned audience should be in the returned list */
        Assert.Contains(audiences, current => current == payload.Value);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/audiences with duplicate audience should return 409 #ERROR-F4E2A")]
    public async Task WhenPostClientAudiencesWithDuplicateAudience_ShouldReturnConflict()
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
        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", payload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: assign audience to client first time */
        var content = new AssignClientAudienceScheme
        {
            Value = "https://api.example.com"
        };

        var firstResponse = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", content);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        /* act: attempt to assign the same audience again */
        var secondResponse = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", content);
        var error = await secondResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        Assert.Equal(ClientErrors.ClientAlreadyHasAudience, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/audiences with non-existent client should return 404 #ERROR-2D943")]
    public async Task WhenPostClientAudiencesWithNonExistentClient_ShouldReturnNotFound()
    {
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

        /* arrange: prepare request with a non-existent client ID */
        var nonExistentClientId = Guid.NewGuid().ToString();
        var payload = new AssignClientAudienceScheme
        {
            Value = "https://api.example.com"
        };

        /* act: send POST request for non-existent client */
        var response = await httpClient.PostAsJsonAsync($"api/v1/clients/{nonExistentClientId}/audiences", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ClientErrors.ClientDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /clients/{id}/audiences and generating token should include audiences in JWT")]
    public async Task WhenAssigningAudiencesAndGeneratingToken_ShouldIncludeAudiencesInJwt()
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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: assign first audience */
        var audience1 = new AssignClientAudienceScheme
        {
            Value = "https://api1.example.com"
        };

        var audience1Response = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", audience1);

        Assert.NotNull(audience1Response);
        Assert.Equal(HttpStatusCode.OK, audience1Response.StatusCode);

        /* arrange: assign second audience */
        var audience2 = new AssignClientAudienceScheme
        {
            Value = "https://api2.example.com"
        };

        var audience2Response = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", audience2);

        Assert.NotNull(audience2Response);
        Assert.Equal(HttpStatusCode.OK, audience2Response.StatusCode);

        /* arrange: prepare client credentials for token generation */
        var connectClient = factory.HttpClient;
        var clientCredentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", client.ClientId },
            { "client_secret", client.Secret }
        };

        var content = new FormUrlEncodedContent(clientCredentials);

        /* act: generate token via client_credentials */
        var tokenResponse = await connectClient.PostAsync("api/v1/protocol/open-id/connect/token", content);
        var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<ClientAuthenticationResult>();

        Assert.Equal(HttpStatusCode.OK, tokenResponse.StatusCode);

        Assert.NotNull(tokenResult);
        Assert.NotEmpty(tokenResult.AccessToken);

        /* act: decode JWT to verify audiences */
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenResult.AccessToken);

        /* assert: token should contain both audiences in the 'aud' claim */
        var audienceClaims = token.Claims
            .Where(claim => claim.Type == JwtRegisteredClaimNames.Aud)
            .Select(claim => claim.Value)
            .ToList();

        Assert.NotNull(audienceClaims);
        Assert.NotEmpty(audienceClaims);

        Assert.Contains(audience1.Value, audienceClaims);
        Assert.Contains(audience2.Value, audienceClaims);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/audiences/{audience} should revoke audience successfully")]
    public async Task WhenDeleteClientAudience_ShouldRevokeAudienceSuccessfully()
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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: assign two audiences */
        var audience1 = "orion";
        var audience2 = "sirius";

        var audience1Payload = new AssignClientAudienceScheme { Value = audience1 };
        var audience2Payload = new AssignClientAudienceScheme { Value = audience2 };

        var x = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", audience1Payload);
        var y = await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", audience2Payload);

        /* act: send DELETE request to revoke first audience */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}/audiences/{audience1}");
        var contentString = await response.Content.ReadAsStringAsync();

        var remainingAudiences = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<string>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(remainingAudiences);

        /* assert: only second audience should remain */
        Assert.Single(remainingAudiences);

        Assert.Contains(audience2, remainingAudiences);
        Assert.DoesNotContain(audience1, remainingAudiences);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/audiences/{audience} with non-existent client should return 404 #ERROR-2D943")]
    public async Task WhenDeleteClientAudienceWithNonExistentClient_ShouldReturnNotFound()
    {
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

        /* arrange: prepare request with non-existent client ID */
        var nonExistentClientId = Guid.NewGuid().ToString();
        var audience = "https://api.example.com";

        /* act: send DELETE request for non-existent client */
        var response = await httpClient.DeleteAsync($"api/v1/clients/{nonExistentClientId}/audiences/{Uri.EscapeDataString(audience)}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(ClientErrors.ClientDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /clients/{id}/audiences/{audience} with audience not assigned should return 409 #ERROR-B3F8E")]
    public async Task WhenDeleteClientAudienceWithAudienceNotAssigned_ShouldReturnConflict()
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
        var clientPayload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, $"test-client-{Guid.NewGuid()}")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var clientResponse = await httpClient.PostAsJsonAsync("api/v1/clients", clientPayload);

        Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(clientPayload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, CancellationToken.None);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: assign one audience */
        var assignedAudience = "https://api1.example.com";
        var assignPayload = new AssignClientAudienceScheme { Value = assignedAudience };

        await httpClient.PostAsJsonAsync($"api/v1/clients/{client.Id}/audiences", assignPayload);

        /* act: send DELETE request for non-assigned audience */
        var nonAssignedAudience = "https://api2.example.com";

        var response = await httpClient.DeleteAsync($"api/v1/clients/{client.Id}/audiences/{Uri.EscapeDataString(nonAssignedAudience)}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(ClientErrors.AudienceNotAssigned, error);
    }
}
