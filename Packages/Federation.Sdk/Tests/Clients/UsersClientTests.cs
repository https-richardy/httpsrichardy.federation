namespace HttpsRichardy.Federation.Sdk.TestSuite.Clients;

public sealed class UsersClientTests(FederationProviderFixture server) :
    IClassFixture<FederationProviderFixture>
{
    private readonly HttpClient _httpClient = server.HttpClient;

    [Fact(DisplayName = "[e2e] - when get users with valid authentication should succeed")]
    public async Task WhenGetUsers_WithValidAuthentication_ShouldSucceed()
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

        /* arrange: create the users client and the request to fetch users */
        var usersClient = new UsersClient(_httpClient);
        var request = new UsersFetchParameters();

        /* act: call the get users async method */
        var result = await usersClient.GetUsersAsync(request);

        /* assert: verify that the users were fetched successfully */
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.Items);
    }

    [Fact(DisplayName = "[e2e] - when delete user with valid data should succeed")]
    public async Task WhenDeleteUser_WithValidData_ShouldSucceed()
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

        /* arrange: create a user to be deleted */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-to-delete@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the delete user async method */
        var result = await usersClient.DeleteUserAsync(user.Id);

        /* assert: verify that the user was deleted successfully */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when delete non-existent user should fail")]
    public async Task WhenDeleteNonExistentUser_ShouldFail()
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

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var nonExistentUserId = "user_xSAdj12371lsajd";

        /* act: call the delete user async method */
        var result = await usersClient.DeleteUserAsync(nonExistentUserId);

        /* assert: verify that the delete failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when get user permissions with valid user should succeed")]
    public async Task WhenGetUserPermissions_WithValidUser_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-get-permissions@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the get user permissions async method */
        var result = await usersClient.GetUserPermissionsAsync(user.Id);

        /* assert: verify that the permissions were fetched successfully */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[e2e] - when get user permissions with non-existent user should fail")]
    public async Task WhenGetUserPermissions_WithNonExistentUser_ShouldFail()
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

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var nonExistentUserId = "user_xSAdj12371lsajd";

        /* act: call the get user permissions async method */
        var result = await usersClient.GetUserPermissionsAsync(nonExistentUserId);

        /* assert: verify that the get user permissions failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when get user groups with valid user should succeed")]
    public async Task WhenGetUserGroups_WithValidUser_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-get-groups@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the get user groups async method */
        var result = await usersClient.GetUserGroupsAsync(user.Id);

        /* assert: verify that the groups were fetched successfully */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact(DisplayName = "[e2e] - when get user groups with non-existent user should fail")]
    public async Task WhenGetUserGroups_WithNonExistentUser_ShouldFail()
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

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);
        var nonExistentUserId = "user_xSAdj12371lsajd";

        /* act: call the get user groups async method */
        var result = await usersClient.GetUserGroupsAsync(nonExistentUserId);

        /* assert: verify that the get user groups failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user to group with valid data should succeed")]
    public async Task WhenAssignUserToGroup_WithValidData_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-to-assign-group@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.user.assignment" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* act: call the assign user to group async method */
        var result = await usersClient.AssignUserGroupAsync(user.Id, groupResult.Data.Id);

        /* assert: verify that the user was assigned to the group successfully */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when assign user to group with non-existent user should fail")]
    public async Task WhenAssignUserToGroup_WithNonExistentUser_ShouldFail()
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

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.non-existent.user.assignment" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* arrange: create the users client and the context for assigning the user to the group */
        var usersClient = new UsersClient(_httpClient);

        /* act: call the assign user to group async method */
        var result = await usersClient.AssignUserGroupAsync("user_xSAdj12371lsajd", groupResult.Data.Id);

        /* assert: verify that the assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user to group with non-existent group should fail")]
    public async Task WhenAssignUserToGroup_WithNonExistentGroup_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-for-non-existent-group-assignment@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the assign user to group async method */
        var result = await usersClient.AssignUserGroupAsync(user.Id, "group_Kjdajmfg12863h");

        /* assert: verify that the assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(GroupErrors.GroupDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user to group when user already in group should fail")]
    public async Task WhenAssignUserToGroup_WhenUserAlreadyInGroup_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-already-in-group@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.duplicate.user.assignment" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* act: call the assign user to group async method twice */
        await usersClient.AssignUserGroupAsync(user.Id, groupResult.Data.Id);

        var result = await usersClient.AssignUserGroupAsync(user.Id, groupResult.Data.Id);

        /* assert: verify that the second assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(UserErrors.UserAlreadyInGroup, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user permission with valid data should succeed")]
    public async Task WhenAssignUserPermission_WithValidData_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-to-assign-permission@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.user.assignment" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the users client and the context for assigning the permission to the user */

        /* act: call the assign user permission async method */
        var result = await usersClient.AssignUserPermissionAsync(user.Id, permissionResult.Data.Name);

        /* assert: verify that the permission was assigned to the user successfully */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when assign user permission with non-existent user should fail")]
    public async Task WhenAssignUserPermission_WithNonExistentUser_ShouldFail()
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

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.non-existent.user.assignment" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the users client and the context for assigning the permission to the user */
        var usersClient = new UsersClient(_httpClient);

        /* act: call the assign user permission async method */
        var result = await usersClient.AssignUserPermissionAsync("user_xSAdj12371lsajd", permissionResult.Data.Name);

        /* assert: verify that the assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user permission with non-existent permission should fail")]
    public async Task WhenAssignUserPermission_WithNonExistentPermission_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-for-non-existent-permission-assignment@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the assign user permission async method */
        var result = await usersClient.AssignUserPermissionAsync(user.Id, "federation.defaults");

        /* assert: verify that the assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(PermissionErrors.PermissionDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign user permission when user already has permission should fail")]
    public async Task WhenAssignUserPermission_WhenUserAlreadyHasPermission_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-already-has-permission@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.duplicate.user.assignment" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* act: call the assign user permission async method twice */
        await usersClient.AssignUserPermissionAsync(user.Id, permissionResult.Data.Name);

        var result = await usersClient.AssignUserPermissionAsync(user.Id, permissionResult.Data.Name);

        /* assert: verify that the second assignment failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserAlreadyHasPermission, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when revoke user permission with valid data should succeed")]
    public async Task WhenRevokeUserPermission_WithValidData_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-to-revoke-permission@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.user.revocation" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        await usersClient.AssignUserPermissionAsync(user.Id, permissionResult.Data.Name);

        /* act: call the revoke user permission async method */
        var result = await usersClient.RevokeUserPermissionAsync(user.Id, permissionResult.Data.Id);

        /* assert: verify that the permission was revoked from the user successfully */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when revoke user permission from non-existent user should fail")]
    public async Task WhenRevokeUserPermission_FromNonExistentUser_ShouldFail()
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

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.non-existent.user.revocation" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);

        /* act: call the revoke user permission async method */
        var result = await usersClient.RevokeUserPermissionAsync("user_xSAdj12371lsajd", permissionResult.Data.Id);

        /* assert: verify that the revocation failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when revoke user permission with non-existent permission should fail")]
    public async Task WhenRevokeUserPermission_WithNonExistentPermission_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-for-non-existent-permission-revocation@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the revoke user permission async method */
        var result = await usersClient.RevokeUserPermissionAsync(user.Id, "permission_nOdj1238712");

        /* assert: verify that the revocation failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(PermissionErrors.PermissionDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when revoke user permission when permission not assigned should fail")]
    public async Task WhenRevokeUserPermission_WhenPermissionNotAssigned_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-permission-not-assigned@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a permission */
        var permissionsClient = new PermissionsClient(_httpClient);
        var permission = new PermissionCreationScheme { Name = "federation.defaults.permissions.for.not-assigned.revocation" };

        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* act: call the revoke user permission async method */
        var result = await usersClient.RevokeUserPermissionAsync(user.Id, permissionResult.Data.Id);

        /* assert: verify that the revocation failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.PermissionNotAssigned, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when remove user from group with valid data should succeed")]
    public async Task WhenRemoveUserFromGroup_WithValidData_ShouldSucceed()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-to-remove-from-group@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.user.removal" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        await usersClient.AssignUserGroupAsync(user.Id, groupResult.Data.Id);

        /* act: call the remove user from group async method */
        var result = await usersClient.RemoveUserFromGroupAsync(user.Id, groupResult.Data.Id);

        /* assert: verify that the user was removed from the group successfully */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when remove user from group from non-existent user should fail")]
    public async Task WhenRemoveUserFromGroup_FromNonExistentUser_ShouldFail()
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

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.non-existent.user.removal" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* arrange: create the users client */
        var usersClient = new UsersClient(_httpClient);

        /* act: call the remove user from group async method */
        var result = await usersClient.RemoveUserFromGroupAsync("user_xSAdj12371lsajd", groupResult.Data.Id);

        /* assert: verify that the removal failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when remove user from group from non-existent group should fail")]
    public async Task WhenRemoveUserFromGroup_FromNonExistentGroup_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-for-non-existent-group-removal@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* act: call the remove user from group async method */
        var result = await usersClient.RemoveUserFromGroupAsync(user.Id, "group_lKi123fu29379");

        /* assert: verify that the removal failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(GroupErrors.GroupDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when remove user from group when user not in group should fail")]
    public async Task WhenRemoveUserFromGroup_WhenUserNotInGroup_ShouldFail()
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

        /* arrange: create a user */
        var identityEnrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = "user-not-in-group@federation.com",
            Password = "Password123!"
        };

        var enrollmentResult = await identityClient.CreateIdentityAsync(identityEnrollmentCredentials);

        Assert.True(enrollmentResult.IsSuccess);

        var usersClient = new UsersClient(_httpClient);
        var fetchUsersResult = await usersClient.GetUsersAsync(new() { Username = identityEnrollmentCredentials.Username });

        Assert.True(fetchUsersResult.IsSuccess);
        Assert.NotNull(fetchUsersResult.Data);
        Assert.NotEmpty(fetchUsersResult.Data.Items);

        var user = fetchUsersResult.Data.Items.FirstOrDefault();

        Assert.NotNull(user);

        /* arrange: create a group */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme { Name = "federation.defaults.groups.for.not-in-group.removal" };

        var groupResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* act: call the remove user from group async method */
        var result = await usersClient.RemoveUserFromGroupAsync(user.Id, groupResult.Data.Id);

        /* assert: verify that the removal failed and the correct error was returned */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(UserErrors.UserNotInGroup, result.Error);
    }
}
