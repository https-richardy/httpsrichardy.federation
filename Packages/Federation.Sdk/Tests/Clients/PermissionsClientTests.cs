namespace HttpsRichardy.Federation.Sdk.TestSuite.Clients;

public sealed class PermissionsClientTests(FederationProviderFixture server) :
    IClassFixture<FederationProviderFixture>
{
    private readonly HttpClient _httpClient = server.HttpClient;

    [Fact(DisplayName = "[e2e] - when create permission with valid data should succeed")]
    public async Task WhenCreatePermission_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and the permission to create */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme
        {
            Name = "federation.defaults.permissions.testing"
        };

        /* act: call the create permission async method */
        var result = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the permission was created successfully */
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Data);
        Assert.Equal(permission.Name, result.Data.Name);
    }

    [Fact(DisplayName = "[e2e] - when create an already existing permission should fail")]
    public async Task WhenCreateAnAlreadyExistingPermission_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and the permission to create */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme
        {
            Name = "federation.defaults.permissions.already.exists"
        };

        /* act: call the create permission async method twice */
        var firstResult = await permissionsClient.CreatePermissionAsync(permission);
        var secondResult = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the first creation was successful and the second failed with the correct error */
        Assert.True(firstResult.IsSuccess);
        Assert.False(secondResult.IsSuccess);

        Assert.NotNull(secondResult.Error);
        Assert.Equal(PermissionErrors.PermissionAlreadyExists, secondResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when update permission with valid data should succeed")]
    public async Task WhenUpdatePermission_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and a new permission */
        var permissionsClient = new PermissionsClient(_httpClient.WithRealm("master"));
        var permissionToCreate = new PermissionCreationScheme
        {
            Name = "federation.defaults.permissions.to.update"
        };

        var createResult = await permissionsClient.CreatePermissionAsync(permissionToCreate);

        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* arrange: prepare update context with the created permission Id and new name */
        var updateContext = new PermissionUpdateScheme
        {
            PermissionId = createResult.Data.Id,
            Name = "federation.defaults.permissions.updated"
        };

        /* act: call the update permission async method */
        var updateResult = await permissionsClient.UpdatePermissionAsync(updateContext);

        /* assert: verify that the permission was updated successfully */
        Assert.True(updateResult.IsSuccess);
        Assert.NotNull(updateResult.Data);

        Assert.Equal(updateContext.PermissionId.ToString(), updateResult.Data.Id);
        Assert.Equal(updateContext.Name, updateResult.Data.Name);
    }

    [Fact(DisplayName = "[e2e] - when update non-existent permission should fail")]
    public async Task WhenUpdateNonExistentPermission_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and prepare update context for a non-existent permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var updateContext = new PermissionUpdateScheme
        {
            PermissionId = "permission_kdjWSUywsqr7251",
            Name = "federation.defaults.permissions.non.existent"
        };

        /* act: call the update permission async method */
        var updateResult = await permissionsClient.UpdatePermissionAsync(updateContext);

        /* assert: verify that the update failed and the correct error was returned */
        Assert.False(updateResult.IsSuccess);

        Assert.NotNull(updateResult.Error);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, updateResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when delete permission with valid data should succeed")]
    public async Task WhenDeletePermission_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and the permission to create */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme
        {
            Name = "federation.defaults.permissions.to.delete"
        };

        /* act: call the create permission async method */
        var createResult = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the permission was created successfully */
        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* act: call the delete permission async method */
        var deleteResult = await permissionsClient.DeletePermissionAsync(createResult.Data.Id);

        /* assert: verify that the permission was deleted successfully */
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when delete non-existent permission should fail")]
    public async Task WhenDeleteNonExistentPermission_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permissionId = "permission_kdjWSUywsqr7251";

        /* act: call the delete permission async method */
        var deleteResult = await permissionsClient.DeletePermissionAsync(permissionId);

        /* assert: verify that the delete failed and the correct error was returned */
        Assert.False(deleteResult.IsSuccess);

        Assert.NotNull(deleteResult.Error);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, deleteResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when get permissions should succeed")]
    public async Task WhenGetPermissions_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the permissions client and parameters for fetching */
        var permissionsClient = new PermissionsClient(_httpClient);
        var parameters = new PermissionsFetchParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        /* act: call the get permissions async method */
        var result = await permissionsClient.GetPermissionsAsync(parameters);

        /* assert: verify that the request was successful and returned data */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Items);
    }
}
