namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class IdentityEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when POST /identity with new username should create identity successfully")]
    public async Task WhenPostIdentityWithNewUsername_ShouldCreateIdentitySuccessfully()
    {
        /* arrange: prepare unique username and password */
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var username = $"john.doe@email.com";
        var password = "TestPassword123!";

        var credentials = new IdentityEnrollmentCredentials
        {
            Username = username,
            Password = password
        };

        /* act: send POST request to create identity */
        var response = await httpClient.PostAsJsonAsync("api/v1/identity", credentials);
        var result = await response.Content.ReadFromJsonAsync<UserDetailsScheme>();

        /* assert: response should be 201 Created and returned user details should match */
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);

        /* assert: returned user details are valid, with a non-empty ID and matching username */
        Assert.Equal(username, result.Username);
        Assert.False(string.IsNullOrWhiteSpace(result.Id));
    }

    [Fact(DisplayName = "[e2e] - when POST identity/authenticate with valid credentials should return access token 'n refresh token")]
    public async Task WhenPostIdentityAuthenticateWithValidCredentials_ShouldReturnAccessTokenAndRefreshToken()
    {
        /* arrange: define credentials for an existing user */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        /* act: send POST request to authenticate endpoint */
        var response = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        /* assert: response and result should not be null, and status code should be OK */
        Assert.NotNull(response);
        Assert.NotNull(result);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        /* assert: both access token and refresh token should be returned and not empty */
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
    }

    [Fact(DisplayName = "[e2e] - when valid refresh token is provided should return new access token")]
    public async Task WhenValidRefreshTokenProvided_ShouldReturnNewAccessToken()
    {
        /* arrange: authenticate user and get refresh token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        /* act: use refresh token to get a new access token */
        var refreshRequest = new SessionTokenRenewalScheme
        {
            RefreshToken = authenticationResult!.RefreshToken
        };

        var refreshResponse = await httpClient.PostAsJsonAsync("api/v1/identity/refresh-token", refreshRequest);
        var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        /* assert: response should be OK and new access token should be returned */
        Assert.NotNull(refreshResponse);
        Assert.NotNull(refreshResult);

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        /* assert: both access token and refresh token should be returned and not empty */
        Assert.NotEmpty(refreshResult.AccessToken);
        Assert.NotEmpty(refreshResult.RefreshToken);
    }

    [Fact(DisplayName = "[e2e] - when invalid refresh token is provided should return 401 #ERROR-32B37")]
    public async Task WhenInvalidRefreshTokenProvided_ShouldReturnBadRequest()
    {
        /* arrange: prepare an invalid refresh token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var refreshRequest = new SessionTokenRenewalScheme
        {
            RefreshToken = "this-is-an-invalid-token"
        };

        /* act: call refresh-token endpoint */
        var refreshResponse = await httpClient.PostAsJsonAsync("api/v1/identity/refresh-token", refreshRequest);
        var error = await refreshResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 401 and error should match ERR-AUT-410 */
        Assert.Equal(HttpStatusCode.Unauthorized, refreshResponse.StatusCode);

        Assert.NotNull(error);
        Assert.Equal(AuthenticationErrors.InvalidTokenFormat, error);
    }

    [Fact(
        DisplayName = "[e2e] - when refresh token is revoked should return 400 BadRequest #ERROR-2C0D9",
        Skip = "test temporarily skipped — pending fix for revoked refresh token scenario (#ERROR-2C0D9)"
    )]
    public async Task WhenRefreshTokenRevoked_ShouldReturnBadRequest()
    {
        /* arrange: authenticate user and get access & refresh tokens */
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
        Assert.NotEmpty(authenticationResult.RefreshToken);

        /* arrange: revoke the refresh token using ISecurityTokenService */
        var scope = factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ISecurityTokenService>();

        var refreshToken = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, authenticationResult.RefreshToken)
            .With(token => token.Type, TokenType.Refresh)
            .Create();

        var revokeResult = await tokenService.RevokeRefreshTokenAsync(refreshToken);

        Assert.True(revokeResult.IsSuccess, "Refresh token should be revoked successfully");

        /* act: attempt to renew session with the revoked refresh token */
        var refreshRequest = _fixture.Build<SessionTokenRenewalScheme>()
            .With(payload => payload.RefreshToken, authenticationResult.RefreshToken)
            .Create();

        var refreshResponse = await httpClient.PostAsJsonAsync("api/v1/identity/refresh-token", refreshRequest);
        var error = await refreshResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: endpoint should reject revoked token */
        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
        Assert.NotNull(error);

        Assert.Equal(AuthenticationErrors.InvalidRefreshToken, error);
    }

    [Fact(DisplayName = "[e2e] - when GET /identity/principal with valid token should return authenticated user details")]
    public async Task WhenGetPrincipalWithValidToken_ShouldReturnAuthenticatedUserDetails()
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

        /* arrange: configure client with access token */
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        /* act: send GET request to principal endpoint */
        var principalResponse = await httpClient.GetAsync("api/v1/identity/principal");
        var principalResult = await principalResponse.Content.ReadFromJsonAsync<PrincipalDetailsScheme>();

        /* assert: response should be 200 OK and principal details should be returned */
        Assert.Equal(HttpStatusCode.OK, principalResponse.StatusCode);
        Assert.NotNull(principalResult);

        /* assert: principal details should contain valid user information */
        Assert.False(string.IsNullOrWhiteSpace(principalResult.Id));
        Assert.Equal(credentials.Username, principalResult.Username);

        /* assert: principal should have created timestamp */
        Assert.NotEqual(default, principalResult.CreatedAt);

        /* assert: permissions and groups collections should be initialized */
        Assert.NotNull(principalResult.Permissions);
        Assert.NotNull(principalResult.Groups);
    }

    [Fact(DisplayName = "[e2e] - when GET /identity/principal without authentication should return 401 Unauthorized")]
    public async Task WhenGetPrincipalWithoutAuthentication_ShouldReturnUnauthorized()
    {
        /* arrange: create client without authentication token */
        var httpClient = factory.HttpClient;

        httpClient.DefaultRequestHeaders.Remove("Authorization");
        httpClient.WithRealmHeader("master");

        /* act: send GET request to principal endpoint without token */
        var principalResponse = await httpClient.GetAsync("api/v1/identity/principal");

        /* assert: response should be 401 Unauthorized */
        Assert.Equal(HttpStatusCode.Unauthorized, principalResponse.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when GET /identity/principal with invalid token should return 401 Unauthorized")]
    public async Task WhenGetPrincipalWithInvalidToken_ShouldReturnUnauthorized()
    {
        /* arrange: create client with invalid token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token-here");

        /* act: send GET request to principal endpoint with invalid token */
        var principalResponse = await httpClient.GetAsync("api/v1/identity/principal");

        /* assert: response should be 401 Unauthorized */
        Assert.Equal(HttpStatusCode.Unauthorized, principalResponse.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when GET /identity/principal should return user with permissions and groups")]
    public async Task WhenGetPrincipal_ShouldReturnUserWithPermissionsAndGroups()
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

        /* arrange: configure client with access token */
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        /* act: send GET request to principal endpoint */
        var principalResponse = await httpClient.GetAsync("api/v1/identity/principal");
        var principalResult = await principalResponse.Content.ReadFromJsonAsync<PrincipalDetailsScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, principalResponse.StatusCode);
        Assert.NotNull(principalResult);

        /* assert: principal should contain permissions collection */
        Assert.NotNull(principalResult.Permissions);

        /* assert: principal should contain groups collection */
        Assert.NotNull(principalResult.Groups);

        /* assert: if permissions exist, they should have valid structure */
        if (principalResult.Permissions.Count > 0)
        {
            var firstPermission = principalResult.Permissions.First();

            Assert.False(string.IsNullOrWhiteSpace(firstPermission.Id));
            Assert.False(string.IsNullOrWhiteSpace(firstPermission.Name));
        }

        /* assert: if groups exist, they should have valid structure */
        if (principalResult.Groups.Count > 0)
        {
            var firstGroup = principalResult.Groups.First();

            Assert.False(string.IsNullOrWhiteSpace(firstGroup.Id));
            Assert.False(string.IsNullOrWhiteSpace(firstGroup.Name));
        }
    }
}
