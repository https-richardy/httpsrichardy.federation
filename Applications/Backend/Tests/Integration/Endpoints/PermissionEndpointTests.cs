namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class PermissionEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when GET /permissions should return paginated list of permissions")]
    public async Task WhenGetPermissions_ShouldReturnPaginatedListOfPermissions()
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

        /* act: send GET request to retrieve permissions */
        var response = await httpClient.GetAsync("api/v1/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<Pagination<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);

        /* assert: pagination should have items */
        Assert.NotNull(permissions.Items);
        Assert.True(permissions.Total >= 0);
    }

    [Fact(DisplayName = "[e2e] - when POST /permissions with valid data should create permission successfully")]
    public async Task WhenPostPermissionsWithValidData_ShouldCreatePermissionSuccessfully()
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

        /* arrange: prepare request to create a new permission */
        var payload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        /* act: send POST request to create permission */
        var response = await httpClient.PostAsJsonAsync("api/v1/permissions", payload);
        var permission = await response.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        /* assert: response should be 201 Created */
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(permission);

        /* assert: returned permission details should match */
        Assert.Equal(payload.Name, permission.Name);
        Assert.False(string.IsNullOrWhiteSpace(permission.Id));
    }

    [Fact(DisplayName = "[e2e] - when POST /permissions with duplicate name should return 409 #ERROR-93F4A")]
    public async Task WhenPostPermissionsWithDuplicateName_ShouldReturnConflict()
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

        /* arrange: create a permission first */
        var payload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var firstResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", payload);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        /* act: attempt to create permission with same name */
        var response = await httpClient.PostAsJsonAsync("api/v1/permissions", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionAlreadyExists, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /permissions in a non-master realm with reserved system name should return 409 #ERROR-7B1E2")]
    public async Task WhenPostPermissionsWithReservedSystemNameInNonMasterRealm_ShouldReturnConflict()
    {
        /* arrange: authenticate in master realm */
        var masterClient = factory.HttpClient.WithRealmHeader("master");
        var masterCredentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var masterAuthenticationResponse = await masterClient.PostAsJsonAsync("api/v1/identity/authenticate", masterCredentials);
        var masterAuthenticationResult = await masterAuthenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(masterAuthenticationResult);
        Assert.NotEmpty(masterAuthenticationResult.AccessToken);

        masterClient.WithAuthorization(masterAuthenticationResult.AccessToken);

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await masterClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: authenticate realm via OAuth 2.0 client_credentials */
        var oauthCredentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", realm.ClientId },
            { "client_secret", realm.ClientSecret }
        };

        var oauthContent = new FormUrlEncodedContent(oauthCredentials);
        var connectClient = factory.HttpClient;

        var oauthResponse = await connectClient.PostAsync("api/v1/protocol/open-id/connect/token", oauthContent);
        var oauthResult = await oauthResponse.Content.ReadFromJsonAsync<ClientAuthenticationResult>();

        Assert.Equal(HttpStatusCode.OK, oauthResponse.StatusCode);

        Assert.NotNull(oauthResult);
        Assert.NotEmpty(oauthResult.AccessToken);

        var realmClient = factory.HttpClient.WithRealmHeader(realm.Name);

        realmClient.WithAuthorization(oauthResult.AccessToken);

        /* act: attempt to create a permission using a reserved system name */
        var payload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, Permissions.ViewRealms)
            .Create();

        var response = await realmClient.PostAsJsonAsync("api/v1/permissions", payload);

        /* assert: response should be 409 Conflict */
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<Error>();

        Assert.NotNull(error);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionNameIsReserved, error);
    }

    [Fact(DisplayName = "[e2e] - when PUT /permissions/{id} with valid data should update permission successfully")]
    public async Task WhenPutPermissionsWithValidData_ShouldUpdatePermissionSuccessfully()
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

        /* arrange: create a new permission */
        var createPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", createPayload);
        var permission = await createResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: prepare request to update permission */
        var updatePayload = _fixture.Build<PermissionUpdateScheme>()
            .With(update => update.Name, $"updated.permission.{Guid.NewGuid()}")
            .Create();

        /* act: send PUT request to update permission */
        var response = await httpClient.PutAsJsonAsync($"api/v1/permissions/{permission.Id}", updatePayload);
        var updatedPermission = await response.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedPermission);

        /* assert: permission details should be updated */
        Assert.Equal(permission.Id, updatedPermission.Id);
        Assert.Equal(updatePayload.Name, updatedPermission.Name);
    }

    [Fact(DisplayName = "[e2e] - when PUT /permissions/{id} with reserved system name should return 409 #ERROR-7B1E2")]
    public async Task WhenPutPermissionsWithReservedSystemName_ShouldReturnConflict()
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

        /* arrange: create a custom permission */
        var createPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", createPayload);
        var permission = await createResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* act: attempt to rename it to a reserved system permission */
        var updatePayload = _fixture.Build<PermissionUpdateScheme>()
            .With(update => update.Name, Permissions.ViewRealms)
            .Create();

        var response = await httpClient.PutAsJsonAsync($"api/v1/permissions/{permission.Id}", updatePayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionNameIsReserved, error);
    }

    [Fact(DisplayName = "[e2e] - when PUT /permissions/{id} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenPutPermissionsWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();
        var payload = _fixture.Build<PermissionUpdateScheme>()
            .With(permission => permission.Name, $"updated.permission.{Guid.NewGuid()}")
            .Create();

        /* act: send PUT request to update non-existent permission */
        var response = await httpClient.PutAsJsonAsync($"api/v1/permissions/{nonExistentPermissionId}", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /permissions/{id} with valid permission should delete permission successfully")]
    public async Task WhenDeletePermissionsWithValidPermission_ShouldDeletePermissionSuccessfully()
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

        /* arrange: create a new permission to delete */
        var payload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", payload);
        var permission = await createResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* act: send DELETE request to remove permission */
        var response = await httpClient.DeleteAsync($"api/v1/permissions/{permission.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /permissions/{id} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenDeletePermissionsWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent permission */
        var response = await httpClient.DeleteAsync($"api/v1/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }
}
