namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class UserEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when GET /users should return paginated list of users")]
    public async Task WhenGetUsers_ShouldReturnPaginatedListOfUsers()
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

        /* act: send GET request to retrieve users */
        var response = await httpClient.GetAsync("api/v1/users");
        var users = await response.Content.ReadFromJsonAsync<Pagination<UserDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(users);

        /* assert: pagination should have items */
        Assert.NotNull(users.Items);
        Assert.True(users.Total >= 0);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id} with valid user should delete user successfully")]
    public async Task WhenDeleteUserWithValidUser_ShouldDeleteUserSuccessfully()
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

        /* arrange: create a new user to delete */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.to.delete.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* act: send DELETE request to remove user */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id} with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenDeleteUserWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();

        /* act: send DELETE request for non-existent user */
        var response = await httpClient.DeleteAsync($"api/v1/users/{nonExistentUserId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/permissions should return user's assigned permissions")]
    public async Task WhenGetUserPermissions_ShouldReturnAssignedPermissions()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.permissions.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

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

            var assignPermissionPayload = new AssignUserPermissionScheme
            {
                PermissionName = permission.Name
            };

            var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);
            Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);
        }

        /* act: send GET request to retrieve user's permissions */
        var response = await httpClient.GetAsync($"api/v1/users/{user.Id}/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);

        foreach (var permissionName in permissionNames)
        {
            Assert.Contains(permissions, permission => permission.Name == permissionName);
        }
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/permissions should return both direct and inherited group permissions")]
    public async Task WhenGetUserPermissions_ShouldReturnDirectAndInheritedPermissions()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.inherited.permissions.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a direct permission */
        var directPermissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.direct.{Guid.NewGuid()}")
            .Create();

        var directPermissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", directPermissionPayload);
        var directPermission = await directPermissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(directPermission);
        Assert.Equal(HttpStatusCode.Created, directPermissionResponse.StatusCode);

        /* arrange: assign direct permission to user */
        var assignDirectPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = directPermission.Name
        };

        var assignDirectPermissionResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignDirectPermissionPayload);
        Assert.Equal(HttpStatusCode.NoContent, assignDirectPermissionResponse.StatusCode);

        /* arrange: create group and inherited permission */
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        var inheritedPermissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.inherited.{Guid.NewGuid()}")
            .Create();

        var inheritedPermissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", inheritedPermissionPayload);
        var inheritedPermission = await inheritedPermissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(inheritedPermission);
        Assert.Equal(HttpStatusCode.Created, inheritedPermissionResponse.StatusCode);

        var assignGroupPermissionPayload = new AssignGroupPermissionScheme
        {
            PermissionName = inheritedPermission.Name
        };

        var assignGroupPermissionResponse = await httpClient.PostAsJsonAsync($"api/v1/groups/{group.Id}/permissions", assignGroupPermissionPayload);
        Assert.Equal(HttpStatusCode.OK, assignGroupPermissionResponse.StatusCode);

        /* arrange: assign user to group */
        var assignUserToGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = group.Id
        };

        var assignUserToGroupResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignUserToGroupPayload);
        Assert.Equal(HttpStatusCode.NoContent, assignUserToGroupResponse.StatusCode);

        /* act: request user permissions */
        var response = await httpClient.GetAsync($"api/v1/users/{user.Id}/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: should return both direct and inherited permissions */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);

        Assert.Contains(permissions, permission => permission.Name == directPermission.Name);
        Assert.Contains(permissions, permission => permission.Name == inheritedPermission.Name);
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/permissions and user has no groups should still return direct permissions")]
    public async Task WhenGetUserPermissionsWithUserWithoutGroups_ShouldReturnDirectPermissions()
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

        /* arrange: create a new user without assigning any groups */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.no.groups.permissions.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create and assign a direct permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.direct.only.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = permission.Name
        };

        var assignPermissionResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);
        Assert.Equal(HttpStatusCode.NoContent, assignPermissionResponse.StatusCode);

        /* act: request user permissions */
        var response = await httpClient.GetAsync($"api/v1/users/{user.Id}/permissions");
        var permissions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<PermissionDetailsScheme>>();

        /* assert: endpoint should work and return direct permissions even without group membership */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(permissions);
        Assert.Contains(permissions, assigned => assigned.Name == permission.Name);
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/permissions with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenGetUserPermissionsWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();

        /* act: send GET request for non-existent user's permissions */
        var response = await httpClient.GetAsync($"api/v1/users/{nonExistentUserId}/permissions");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist.Code, error.Code);
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/groups should return user's assigned groups")]
    public async Task WhenGetUserGroups_ShouldReturnAssignedGroups()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.groups.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create and assign multiple groups */
        var groupNames = new List<string>();

        for (int index = 0; index < 2; index++)
        {
            var groupPayload = _fixture.Build<GroupCreationScheme>()
                .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
                .Create();

            var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
            var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

            Assert.NotNull(group);
            groupNames.Add(group.Name);

            var assignGroupPayload = new AssignUserToGroupScheme
            {
                GroupId = group.Id
            };

            var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);
            Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);
        }

        /* act: send GET request to retrieve user's groups */
        var response = await httpClient.GetAsync($"api/v1/users/{user.Id}/groups");
        var groups = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<GroupBasicDetailsScheme>>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(groups);

        foreach (var groupName in groupNames)
        {
            Assert.Contains(groups, group => group.Name == groupName);
        }
    }

    [Fact(DisplayName = "[e2e] - when GET /users/{id}/groups with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenGetUserGroupsWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();

        /* act: send GET request for non-existent user's groups */
        var response = await httpClient.GetAsync($"api/v1/users/{nonExistentUserId}/groups");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist.Code, error.Code);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/groups with valid group should assign user to group successfully")]
    public async Task WhenPostUserGroupsWithValidGroup_ShouldAssignUserToGroupSuccessfully()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.assign.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new group */
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        /* arrange: prepare request to assign user to group */
        var assignGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = group.Id
        };

        /* act: send POST request to assign user to group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/groups with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenPostUserGroupsWithNonExistentUser_ShouldReturnNotFound()
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
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();
        var assignGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = group.Id
        };

        /* act: send POST request to assign non-existent user to group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{nonExistentUserId}/groups", assignGroupPayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/groups with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenPostUserGroupsWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.assign.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: prepare request with a non-existent group ID */
        var assignGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = Guid.NewGuid().ToString()
        };

        /* act: send POST request to assign user to non-existent group */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/groups with duplicate group should return 409 #ERROR-33066")]
    public async Task WhenPostUserGroupsWithDuplicateGroup_ShouldReturnConflict()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.duplicate.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new group */
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        /* arrange: assign user to group first time */
        var assignGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = group.Id
        };

        var firstAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);

        Assert.Equal(HttpStatusCode.NoContent, firstAssignResponse.StatusCode);

        /* act: attempt to assign the same group again */
        var secondAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);
        var error = await secondAssignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondAssignResponse.StatusCode);
        Assert.Equal(UserErrors.UserAlreadyInGroup, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/permissions with valid permission should assign permission successfully")]
    public async Task WhenPostUserPermissionsWithValidPermission_ShouldAssignPermissionSuccessfully()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.assign.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: prepare request to assign permission to user */
        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = permission.Name
        };

        /* act: send POST request to assign permission to user */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/permissions with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenPostUserPermissionsWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();
        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = "some.permission"
        };

        /* act: send POST request to assign permission to non-existent user */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{nonExistentUserId}/permissions", assignPermissionPayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/permissions with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenPostUserPermissionsWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.assign.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: prepare request with a non-existent permission name */
        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = $"non.existent.permission.{Guid.NewGuid()}"
        };

        /* act: send POST request to assign non-existent permission to user */
        var response = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /users/{id}/permissions with duplicate permission should return 409 #ERROR-44DEC")]
    public async Task WhenPostUserPermissionsWithDuplicatePermission_ShouldReturnConflict()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.duplicate.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to user first time */
        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = permission.Name
        };

        var firstAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);

        Assert.Equal(HttpStatusCode.NoContent, firstAssignResponse.StatusCode);

        /* act: attempt to assign the same permission again */
        var secondAssignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);
        var error = await secondAssignResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, secondAssignResponse.StatusCode);
        Assert.Equal(UserErrors.UserAlreadyHasPermission, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/permissions/{permissionId} should revoke permission successfully")]
    public async Task WhenDeleteUserPermission_ShouldRevokePermissionSuccessfully()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.revoke.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new permission */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* arrange: assign permission to user */
        var assignPermissionPayload = new AssignUserPermissionScheme
        {
            PermissionName = permission.Name
        };

        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/permissions", assignPermissionPayload);
        Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

        /* act: send DELETE request to revoke permission */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/permissions/{permission.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/permissions/{permissionId} with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenDeleteUserPermissionWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request to revoke permission from non-existent user */
        var response = await httpClient.DeleteAsync($"api/v1/users/{nonExistentUserId}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/permissions/{permissionId} with non-existent permission should return 404 #ERROR-93697")]
    public async Task WhenDeleteUserPermissionWithNonExistentPermission_ShouldReturnNotFound()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.revoke.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: prepare request with a non-existent permission ID */
        var nonExistentPermissionId = Guid.NewGuid().ToString();

        /* act: send DELETE request to revoke non-existent permission */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/permissions/{nonExistentPermissionId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(PermissionErrors.PermissionDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/permissions/{permissionId} with unassigned permission should return 409 #ERROR-C2FB0")]
    public async Task WhenDeleteUserPermissionWithUnassignedPermission_ShouldReturnConflict()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.revoke.permission.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new permission but do not assign it to user */
        var permissionPayload = _fixture.Build<PermissionCreationScheme>()
            .With(permission => permission.Name, $"test.permission.{Guid.NewGuid()}")
            .Create();

        var permissionResponse = await httpClient.PostAsJsonAsync("api/v1/permissions", permissionPayload);
        var permission = await permissionResponse.Content.ReadFromJsonAsync<PermissionDetailsScheme>();

        Assert.NotNull(permission);
        Assert.Equal(HttpStatusCode.Created, permissionResponse.StatusCode);

        /* act: send DELETE request to revoke unassigned permission */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/permissions/{permission.Id}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(UserErrors.PermissionNotAssigned, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/groups/{groupId} should remove user from group successfully")]
    public async Task WhenDeleteUserGroup_ShouldRemoveUserFromGroupSuccessfully()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.remove.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new group */
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        /* arrange: assign user to group */
        var assignGroupPayload = new AssignUserToGroupScheme
        {
            GroupId = group.Id
        };

        var assignResponse = await httpClient.PostAsJsonAsync($"api/v1/users/{user.Id}/groups", assignGroupPayload);
        Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

        /* act: send DELETE request to remove user from group */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/groups/{group.Id}");

        /* assert: response should be 204 No Content */
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/groups/{groupId} with non-existent user should return 404 #ERROR-E6B32")]
    public async Task WhenDeleteUserGroupWithNonExistentUser_ShouldReturnNotFound()
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

        /* arrange: prepare request with a non-existent user ID */
        var nonExistentUserId = Guid.NewGuid().ToString();
        var nonExistentGroupId = Guid.NewGuid().ToString();

        /* act: send DELETE request to remove non-existent user from group */
        var response = await httpClient.DeleteAsync($"api/v1/users/{nonExistentUserId}/groups/{nonExistentGroupId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(UserErrors.UserDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/groups/{groupId} with non-existent group should return 404 #ERROR-4D2E2")]
    public async Task WhenDeleteUserGroupWithNonExistentGroup_ShouldReturnNotFound()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.remove.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: prepare request with a non-existent group ID */
        var nonExistentGroupId = Guid.NewGuid().ToString();

        /* act: send DELETE request to remove user from non-existent group */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/groups/{nonExistentGroupId}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 404 Not Found */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(GroupErrors.GroupDoesNotExist, error);
    }

    [Fact(DisplayName = "[e2e] - when DELETE /users/{id}/groups/{groupId} with unassigned group should return 409 #ERROR-0E56E")]
    public async Task WhenDeleteUserGroupWithUnassignedGroup_ShouldReturnConflict()
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

        /* arrange: create a new user */
        var enrollmentCredentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.remove.group.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var enrollmentResponse = await httpClient.PostAsJsonAsync("api/v1/identity", enrollmentCredentials);
        var user = await enrollmentResponse.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(user);
        Assert.Equal(HttpStatusCode.Created, enrollmentResponse.StatusCode);

        /* arrange: create a new group but do not assign user to it */
        var groupPayload = _fixture.Build<GroupCreationScheme>()
            .With(group => group.Name, $"test-group-{Guid.NewGuid()}")
            .Create();

        var groupResponse = await httpClient.PostAsJsonAsync("api/v1/groups", groupPayload);
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupDetailsScheme>();

        Assert.NotNull(group);
        Assert.Equal(HttpStatusCode.Created, groupResponse.StatusCode);

        /* act: send DELETE request to remove user from unassigned group */
        var response = await httpClient.DeleteAsync($"api/v1/users/{user.Id}/groups/{group.Id}");
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 409 Conflict */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(UserErrors.UserNotInGroup, error);
    }
}
