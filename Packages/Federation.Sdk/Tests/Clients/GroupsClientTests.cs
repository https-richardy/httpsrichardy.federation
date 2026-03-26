namespace HttpsRichardy.Federation.Sdk.TestSuite.Clients;

public sealed class GroupsClientTests(FederationProviderFixture server) :
    IClassFixture<FederationProviderFixture>
{
    private readonly HttpClient _httpClient = server.HttpClient;

    [Fact(DisplayName = "[e2e] - when create group with valid data should succeed")]
    public async Task WhenCreateGroup_WithValidData_ShouldSucceed()
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

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.testing"
        };

        /* act: call the create group async method */
        var result = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Id);

        Assert.Empty(result.Data.Permissions);
        Assert.Equal(group.Name, result.Data.Name);
    }

    [Fact(DisplayName = "[e2e] - when update group with valid data should succeed")]
    public async Task WhenUpdateGroup_WithValidData_ShouldSucceed()
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

        /* arrange: create the groups client */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.to.update"
        };

        var createResult = await groupsClient.CreateGroupAsync(group);

        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* arrange: prepare update context with the created group Id and new name */
        var updateContext = new GroupUpdateScheme
        {
            Id = createResult.Data.Id,
            Name = "federation.defaults.groups.updated"
        };

        /* act: call the update group async method */
        var updateResult = await groupsClient.UpdateGroupAsync(updateContext);

        /* assert: verify that the group was updated successfully */
        Assert.True(updateResult.IsSuccess);
        Assert.NotNull(updateResult.Data);

        Assert.Equal(updateContext.Id, updateResult.Data.Id);
        Assert.Equal(updateContext.Name, updateResult.Data.Name);
    }

    [Fact(DisplayName = "[e2e] - when update non-existent group should fail")]
    public async Task WhenUpdateNonExistentGroup_ShouldFail()
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

        /* arrange: create the groups client and prepare update context for a non-existent group */
        var groupsClient = new GroupsClient(_httpClient);
        var updateContext = new GroupUpdateScheme
        {
            Id = Guid.NewGuid().ToString(),
            Name = "federation.defaults.groups.non.existent"
        };

        /* act: call the update group async method */
        var updateResult = await groupsClient.UpdateGroupAsync(updateContext);

        /* assert: verify that the update failed and the correct error was returned */
        Assert.False(updateResult.IsSuccess);

        Assert.NotNull(updateResult.Error);
        Assert.Equal(GroupErrors.GroupDoesNotExist, updateResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when delete group with valid data should succeed")]
    public async Task WhenDeleteGroup_WithValidData_ShouldSucceed()
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

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.to.delete"
        };

        /* act: call the create group async method */
        var createResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* act: call the delete group async method */
        var deleteResult = await groupsClient.DeleteGroupAsync(createResult.Data.Id);

        /* assert: verify that the group was deleted successfully */
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when delete non-existent group should fail")]
    public async Task WhenDeleteNonExistentGroup_ShouldFail()
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

        /* arrange: create the groups client */
        var groupsClient = new GroupsClient(_httpClient);
        var groupId = "group_X46587S12445";

        /* act: call the delete group async method */
        var deleteResult = await groupsClient.DeleteGroupAsync(groupId);

        /* assert: verify that the delete failed and the correct error was returned */
        Assert.False(deleteResult.IsSuccess);

        Assert.NotNull(deleteResult.Error);
        Assert.Equal(GroupErrors.GroupDoesNotExist, deleteResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when assign permission to group with valid data should succeed")]
    public async Task WhenAssignPermissionToGroup_WithValidData_ShouldSucceed()
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
            Name = "federation.defaults.permissions.to.assign"
        };

        /* act: call the create permission async method */
        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the permission was created successfully */
        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.to.assign.permission"
        };

        /* act: call the create group async method */
        var groupResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* arrange: create the assign permission to group object */
        var parameters = new AssignGroupPermission
        {
            GroupId = groupResult.Data.Id,
            PermissionName = permissionResult.Data.Name
        };

        /* act: call the assign permission to group async method */
        var assignResult = await groupsClient.AssignGroupPermissionAsync(parameters.GroupId, parameters.PermissionName);

        /* assert: verify that the permission was assigned successfully */
        Assert.True(assignResult.IsSuccess);
        Assert.NotNull(assignResult.Data);

        Assert.NotEmpty(assignResult.Data.Permissions);
        Assert.Contains(assignResult.Data.Permissions, permission => permission.Id == permissionResult.Data.Id);
    }

    [Fact(DisplayName = "[e2e] - when revoke permission from group with valid data should succeed")]
    public async Task WhenRevokePermissionFromGroup_WithValidData_ShouldSucceed()
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
            Name = "federation.defaults.permissions.to.revoke"
        };

        /* act: call the create permission async method */
        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the permission was created successfully */
        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.to.revoke.permission"
        };

        /* act: call the create group async method */
        var groupResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* arrange: create the assign permission to group object */
        var parameters = new AssignGroupPermission
        {
            GroupId = groupResult.Data.Id,
            PermissionName = permissionResult.Data.Name
        };

        /* act: call the assign permission to group async method */
        var assignResult = await groupsClient.AssignGroupPermissionAsync(parameters.GroupId, parameters.PermissionName);

        /* assert: verify that the permission was assigned successfully */
        Assert.True(assignResult.IsSuccess);

        /* act: call the revoke permission from group async method */
        var revokeResult = await groupsClient.RevokeGroupPermissionAsync(groupResult.Data.Id, permissionResult.Data.Id);

        /* assert: verify that the permission was revoked successfully */
        Assert.True(revokeResult.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when revoke permission from non-existent group should fail")]
    public async Task WhenRevokePermissionFromNonExistentGroup_ShouldFail()
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

        /* arrange: create the groups client */
        var groupsClient = new GroupsClient(_httpClient);

        var groupId = "group_X46587S12445";
        var permissionId = "permission_kdjWSUywsqr7251";

        /* act: call the revoke permission from group async method */
        var revokeResult = await groupsClient.RevokeGroupPermissionAsync(groupId, permissionId);

        /* assert: verify that the revoke failed and the correct error was returned */
        Assert.False(revokeResult.IsSuccess);

        Assert.NotNull(revokeResult.Error);
        Assert.Equal(GroupErrors.GroupDoesNotExist, revokeResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when revoke non-existent permission from group should fail")]
    public async Task WhenRevokeNonExistentPermissionFromGroup_ShouldFail()
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

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.revoke.non.existent.permission"
        };

        /* act: call the create group async method */
        var groupResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        var permissionId = "permission_kdjWSUywsqr7251";

        /* act: call the revoke permission from group async method */
        var revokeResult = await groupsClient.RevokeGroupPermissionAsync(groupResult.Data.Id, permissionId);

        /* assert: verify that the revoke failed and the correct error was returned */
        Assert.False(revokeResult.IsSuccess);

        Assert.NotNull(revokeResult.Error);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, revokeResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when revoke permission not assigned to group should fail")]
    public async Task WhenRevokePermissionNotAssignedToGroup_ShouldFail()
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
            Name = "federation.defaults.permissions.not.assigned"
        };

        /* act: call the create permission async method */
        var permissionResult = await permissionsClient.CreatePermissionAsync(permission);

        /* assert: verify that the permission was created successfully */
        Assert.True(permissionResult.IsSuccess);
        Assert.NotNull(permissionResult.Data);

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = "federation.defaults.groups.revoke.not.assigned.permission"
        };

        /* act: call the create group async method */
        var groupResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(groupResult.IsSuccess);
        Assert.NotNull(groupResult.Data);

        /* act: call the revoke permission from group async method */
        var revokeResult = await groupsClient.RevokeGroupPermissionAsync(groupResult.Data.Id, permissionResult.Data.Id);

        /* assert: verify that the revoke failed and the correct error was returned */
        Assert.False(revokeResult.IsSuccess);

        Assert.NotNull(revokeResult.Error);
        Assert.Equal(GroupErrors.PermissionNotAssigned, revokeResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when get groups with no parameters should return created group")]
    public async Task WhenGetGroups_WithNoParameters_ShouldReturnCreatedGroup()
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

        /* arrange: create the groups client and the group to create */
        var groupsClient = new GroupsClient(_httpClient);
        var group = new GroupCreationScheme
        {
            Name = $"federation.defaults.groups.getgroups.{Guid.NewGuid()}"
        };

        /* act: call the create group async method */
        var createResult = await groupsClient.CreateGroupAsync(group);

        /* assert: verify that the group was created successfully */
        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);
        Assert.NotNull(createResult.Data.Id);

        /* act: call the get groups async method with no parameters */
        var result = await groupsClient.GetGroupsAsync();

        /* assert: verify that the created group is present in the result */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        Assert.NotEmpty(result.Data.Items);
        Assert.Contains(result.Data.Items, group => group.Id == createResult.Data.Id && group.Name == group.Name);
    }
}