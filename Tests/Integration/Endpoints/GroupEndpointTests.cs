namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class GroupEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when GET /groups should return paginated list of groups")]
    public async Task WhenGetGroups_ShouldReturnPaginatedListOfGroups()
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

        /* act: send GET request to retrieve groups */
        var response = await httpClient.GetAsync("api/v1/groups");
        var groups = await response.Content.ReadFromJsonAsync<Pagination<GroupDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(groups);

        /* assert: pagination should have items */
        Assert.NotNull(groups.Items);
        Assert.True(groups.Total >= 0);
    }

    [Fact(DisplayName = "[e2e] - when POST /groups with valid data should create group successfully")]
    public async Task WhenPostGroupsWithValidData_ShouldCreateGroupSuccessfully()
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

        /* arrange: prepare request to create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        /* act: send POST request to create group */
        var response = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await response.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        /* assert: response should be 201 Created */
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(group);

        /* assert: returned group details should match */
        Assert.Equal(payload.Name, group.Name);
        Assert.False(string.IsNullOrWhiteSpace(group.Id));
    }

    [Fact(DisplayName = "[e2e] - when POST /groups with duplicate name should return 409 #ERROR-1C6F3")]
    public async Task WhenPostGroupsWithDuplicateName_ShouldReturnConflict()
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

        /* arrange: create a group first */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var firstResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        /* act: attempt to create group with same name */
        var response = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(GroupErrors.GroupAlreadyExists, error);
    }

    [Fact(DisplayName = "[e2e] - when PUT /groups/{id} with valid data should update group successfully")]
    public async Task WhenPutGroupsWithValidData_ShouldUpdateGroupSuccessfully()
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

        /* arrange: create a new group */
        var createPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", createPayload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: prepare request to update group */
        var updatePayload = _fixture.Build<GroupUpdateScheme>()
            .With(update => update.Name, $"updated-group-{Guid.NewGuid()}")
            .Create();

        /* act: send PUT request to update group */
        var response = await httpClient.PutAsJsonAsync($"api/v1/groups/{group.Id}", updatePayload);
        var updatedGroup = await response.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(updatedGroup);

        /* assert: group details should be updated */
        Assert.Equal(group.Id, updatedGroup.Id);
        Assert.Equal(updatePayload.Name, updatedGroup.Name);
    }

    [Fact(DisplayName = "[e2e] - when PUT /groups/{id} with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenPutGroupsWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();
        var payload = _fixture.Build<GroupUpdateScheme>()
            .With(group => group.Name, $"updated-group-{Guid.NewGuid()}")
            .Create();

        /* act: send PUT request to update non-existent group */
        var response = await httpClient.PutAsJsonAsync($"api/v1/groups/{nonExistentGroupId}", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id} with valid group should delete group successfully")]
    public async Task WhenDeleteGroupsWithValidGroup_ShouldDeleteGroupSuccessfully()
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

        /* arrange: create a new group to delete */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* act: send DELETE request to remove group */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{group.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id} with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenDeleteGroupsWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent group */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{nonExistentGroupId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when GET /groups/{id}/permissions should return group's assigned permissions")]
    public async Task WhenGetGroupPermissions_ShouldReturnAssignedPermissions()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

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

            var assignPayload = new AssignGroupPermissionScheme
            {
                PermissionName = permission.Name
            };

            var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);
            Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        }

        /* act: send GET request to retrieve group's permissions */
        var response = await httpClient.GetAsync($"api/v1/groups/{group.Id}/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);

        foreach (var permissionName in permissionNames)
        {
            Assert.Contains(permissions, permission => permission.Name == permissionName);
        }
    }

    [Fact(DisplayName = "[e2e] - when GET /groups/{id}/permissions with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenGetGroupPermissionsWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();

        /* act: send GET request for non-existent group's permissions */
        var response = await httpClient.GetAsync($"api/v1/groups/{nonExistentGroupId}/permissions");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist.Code, error.Code);
    }

    [Fact(DisplayName = "[e2e] - when POST /groups/{id}/permissions with valid permission should assign permission successfully")]
    public async Task WhenPostGroupPermissionsWithValidPermission_ShouldAssignPermissionSuccessfully()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: prepare request to assign permission to group */
        var assignPayload = new AssignGroupPermissionScheme
        {
            PermissionName = permission.Name
        };

        /* act: send POST request to assign permission to group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);
        var content = await response.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        /* assert: response should be 200 OK and updated group should be returned */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(content);
        Assert.NotNull(content.Permissions);

        /* assert: the assigned permission should be in the group's permissions list */
        Assert.Contains(content.Permissions, assigned => assigned.Name == permission.Name);
    }

    [Fact(DisplayName = "[e2e] - when POST /groups/{id}/permissions with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenPostGroupPermissionsWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();
        var payload = new AssignGroupPermissionScheme
        {
            PermissionName = "some.permission"
        };

        /* act: send POST request to assign permission to non-existent group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/groups/{nonExistentGroupId}/permissions", payload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /groups/{id}/permissions with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenPostGroupPermissionsWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: prepare request with a non-existent permission name */
        var assignPayload = new AssignGroupPermissionScheme
        {
            PermissionName = $"non.existent.permission.{Guid.NewGuid()}"
        };

        /* act: send POST request to assign non-existent permission to group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /groups/{id}/permissions with duplicate permission should return 409 #ERROR-9C69E")]
    public async Task WhenPostGroupPermissionsWithDuplicatePermission_ShouldReturnConflict()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to group first time */
        var assignPayload = new AssignGroupPermissionScheme
        {
            PermissionName = permission.Name
        };

        var firstAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);

        Assert.Equal(HttpStatusCode.OK, firstAssignResponse.StatusCode);

        /* act: attempt to assign the same permission again */
        var secondAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);
        var error = await secondAssignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondAssignResponse.StatusCode);
        Assert.Equal(GroupErrors.GroupAlreadyHasPermission, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id}/permissions/{permissionId} should revoke permission successfully")]
    public async Task WhenDeleteGroupPermission_ShouldRevokePermissionSuccessfully()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to group */
        var assignPayload = new AssignGroupPermissionScheme
        {
            PermissionName = permission.Name
        };

        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignPayload);
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        /* act: send DELETE request to revoke permission */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{group.Id}/permissions/{permission.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id}/permissions/{permissionId} with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenDeleteGroupPermissionWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request to revoke permission from non-existent group */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{nonExistentGroupId}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id}/permissions/{permissionId} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenDeleteGroupPermissionWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request to revoke non-existent permission */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{group.Id}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /groups/{id}/permissions/{permissionId} with unassigned permission should return 409 #ERROR-C2FB0")]
    public async Task WhenDeleteGroupPermissionWithUnassignedPermission_ShouldReturnConflict()
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

        /* arrange: create a new group */
        var payload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var createResponse = await httpClient.PostAsJsonAsync("api/v1/groups", payload);
        var group = await createResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        /* arrange: create a new permission but do not assign it to group */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* act: send DELETE request to revoke unassigned permission */
        var response = await httpClient.DeleteAsync($"api/v1/groups/{group.Id}/permissions/{permission.Id}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(GroupErrors.PermissionNotAssigned, error);
    }
}
