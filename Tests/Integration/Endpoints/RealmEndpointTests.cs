namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class RealmEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when POST /realms/{id}/permissions with valid permission should assign permission successfully")]
    public async Task WhenPostRealmPermissionsWithValidPermission_ShouldAssignPermissionSuccessfully()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: prepare request to assign permission to realm */
        var assignPermissionPayload = new AssignRealmPermissionScheme
        {
            PermissionName = permission.Name
        };

        /* act: send POST request to assign permission to realm */
        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", assignPermissionPayload);
        var assignedPermissions = await assignResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK and permissions list should be returned */
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        Assert.NotNull(assignedPermissions);

        /* assert: the assigned permission should be in the returned list */
        Assert.Contains(assignedPermissions, p => p.Name == permission.Name);
    }

    [Fact(DisplayName = "[e2e] - when POST /realms/{id}/permissions with non-existent realm should return 404 #ERROR-2FB9A")]
    public async Task WhenPostRealmPermissionsWithNonExistentRealm_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent realm ID */
        var nonExistentRealmId = Guid.NewGuid().ToString();
        var assignPermissionPayload = new AssignRealmPermissionScheme
        {
            PermissionName = "some.permission"
        };

        /* act: send POST request to assign permission to non-existent realm */
        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/realms/{nonExistentRealmId}/permissions", assignPermissionPayload);
        var error = await assignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, assignResponse.StatusCode);
        Assert.Equal(RealmErrors.RealmDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /realms/{id}/permissions with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenPostRealmPermissionsWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: prepare request with a non-existent permission name */
        var assignPermissionPayload = new AssignRealmPermissionScheme
        {
            PermissionName = $"non.existent.permission.{Guid.NewGuid()}"
        };

        /* act: send POST request to assign non-existent permission to realm */
        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", assignPermissionPayload);
        var error = await assignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, assignResponse.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /realms/{id}/permissions with duplicate permission should return 409 #ERROR-F23E2")]
    public async Task WhenPostRealmPermissionsWithDuplicatePermission_ShouldReturnConflict()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to realm first time */
        var assignPermissionPayload = new AssignRealmPermissionScheme
        {
            PermissionName = permission.Name
        };

        var firstAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", assignPermissionPayload);

        Assert.Equal(HttpStatusCode.OK, firstAssignResponse.StatusCode);

        /* act: attempt to assign the same permission again */
        var secondAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", assignPermissionPayload);
        var error = await secondAssignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondAssignResponse.StatusCode);
        Assert.Equal(RealmErrors.RealmAlreadyHasPermission, error);
    }

    [Fact(DisplayName = "[e2e] - when GET /realms/{id}/permissions should return realm's assigned permissions")]
    public async Task WhenGetRealmPermissions_ShouldReturnAssignedPermissions()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: create and assign multiple permissions */
        var permissionNames = new List<string>();

        for (int index = 0; index < 3; index++)
        {
            var permissionPayload = _fixture.Build<PermissionCreationScheme>()
                .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
                .Create();

            var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
            var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

            Assert.NotNull(permission);

            permissionNames.Add(permission.Name);

            var assignPayload = new AssignRealmPermissionScheme { PermissionName = permission.Name };

            await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", assignPayload);
        }

        /* act: send GET request to retrieve realm's permissions */
        var getResponse = await httpClient.GetAsync($"api/v1/realms/{realm.Id}/permissions");
        var permissions = await getResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.NotNull(permissions);

        foreach (var permissionName in permissionNames)
        {
            Assert.Contains(permissions, permission => permission.Name == permissionName);
        }
    }

    [Fact(DisplayName = "[e2e] - when GET /realms/{id}/permissions with non-existent realm should return 404 #ERROR-2FB9A")]
    public async Task WhenGetRealmPermissionsWithNonExistentRealm_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent realm ID */
        var nonExistentRealmId = Guid.NewGuid().ToString();

        /* act: send GET request for non-existent realm's permissions */
        var response = await httpClient.GetAsync($"api/v1/realms/{nonExistentRealmId}/permissions");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */

        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(RealmErrors.RealmDoesNotExist.Code, error.Code);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /realms/{id}/permissions/{permissionId} should revoke permission successfully")]
    public async Task WhenDeleteRealmPermission_ShouldRevokePermissionSuccessfully()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to realm */
        var payload = new AssignRealmPermissionScheme { PermissionName = permission.Name };
        var response = await httpClient.PostAsJsonAsync($"api/v1/realms/{realm.Id}/permissions", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        /* act: send DELETE request to revoke permission from realm */
        var deleteResponse = await httpClient.DeleteAsync($"api/v1/realms/{realm.Id}/permissions/{permission.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        /* assert: verify permission is no longer in realm's permissions list */
        var httpResponse = await httpClient.GetAsync($"api/v1/realms/{realm.Id}/permissions");
        var permissions = await httpResponse.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        Assert.NotNull(permissions);
        Assert.DoesNotContain(permissions, p => p.Id == permission.Id);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /realms/{id}/permissions/{permissionId} with non-existent realm should return 404 #ERROR-2FB9A")]
    public async Task WhenDeleteRealmPermissionWithNonExistentRealm_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent realm ID */
        var nonExistentRealmId = Guid.NewGuid().ToString();
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent realm */
        var response = await httpClient.DeleteAsync($"api/v1/realms/{nonExistentRealmId}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(RealmErrors.RealmDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /realms/{id}/permissions/{permissionId} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenDeleteRealmPermissionWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new realm */
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .Create();

        var response = await httpClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await response.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request with non-existent permission */
        var httpResponse = await httpClient.DeleteAsync($"api/v1/realms/{realm.Id}/permissions/{nonExistentPermissionId}");
        var error = await httpResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, httpResponse.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }
}
