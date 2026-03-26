namespace HttpsRichardy.Federation.Sdk.TestSuite.Clients;

public sealed class IdentityClientTests(FederationProviderFixture server) :
    IClassFixture<FederationProviderFixture>
{
    private readonly HttpClient _httpClient = server.HttpClient;

    [Fact(DisplayName = "[e2e] - when authenticate with valid credentials should succeed")]
    public async Task WhenAuthenticate_WithValidCredentials_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var result = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        /* assert: verify that both access and refresh tokens are returned and not empty */
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
    }

    [Fact(DisplayName = "[e2e] - when authenticate with non-existent user should return #ERROR-A7E7C")]
    public async Task WhenAuthenticate_WithNonExistentUser_ShouldReturnInvalidCredentials()
    {
        /* arrange: create an identity client with the proper realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));

        /* arrange: define credentials for a user that does not exist */
        var credentials = new AuthenticationCredentials
        {
            Username = "non.existent.user",
            Password = "somepassword"
        };

        /* act: attempt to authenticate the non-existent user */
        var result = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication failed */
        Assert.False(result.IsSuccess);
        Assert.Equal(AuthenticationErrors.InvalidCredentials, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when authenticate with valid user but wrong password should return #ERROR-A7E7C error")]
    public async Task WhenAuthenticate_WithValidUserButWrongPassword_ShouldReturnInvalidCredentials()
    {
        /* arrange: create an identity client with the proper realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));

        /* arrange: define credentials with an existing username but wrong password */
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        /* act: attempt to authenticate with invalid credentials */
        var result = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication failed */
        Assert.True(result.IsFailure);
        Assert.Equal(AuthenticationErrors.InvalidCredentials, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when authenticate without realm header should return #ERROR-B2E42 error")]
    public async Task WhenAuthenticate_WithoutRealmHeader_ShouldReturnRealmHeaderMissing()
    {
        /* arrange: ensure realm header is removed before creating the identity client */
        const string realmHeader = "realm";

        if (_httpClient.DefaultRequestHeaders.Contains(realmHeader))
            _httpClient.DefaultRequestHeaders.Remove(realmHeader);

        /* arrange: create an identity client without setting the realm header */
        var identityClient = new IdentityClient(_httpClient);

        /* arrange: define valid credentials */
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: attempt to authenticate without realm header */
        var result = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication failed */
        Assert.True(result.IsFailure);
        Assert.Equal(RealmErrors.RealmHeaderMissing, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when authenticate with non-existent realm should return #ERROR-2FB9A error")]
    public async Task WhenAuthenticate_WithNonExistentRealm_ShouldReturnRealmDoesNotExist()
    {
        /* arrange: create an identity client with a non-existent realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("non-existent-realm"));

        /* arrange: define valid credentials */
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: attempt to authenticate with non-existent realm */
        var result = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication failed */
        Assert.True(result.IsFailure);
        Assert.Equal(RealmErrors.RealmDoesNotExist, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when create identity with valid credentials should succeed and authenticate successfully")]
    public async Task WhenCreateIdentity_WithValidCredentials_ShouldSucceedAndAuthenticate()
    {
        /* arrange: create an identity client with the proper realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));

        /* arrange: define enrollment credentials for a new user */
        var credentials = new IdentityEnrollmentCredentials
        {
            Username = $"user_{Guid.NewGuid():N}",
            Password = "SuperSecret123!"
        };

        /* act: create the identity */
        var creationResult = await identityClient.CreateIdentityAsync(credentials);

        /* assert: ensure the identity creation was successful */
        Assert.True(creationResult.IsSuccess);

        /* arrange: prepare authentication credentials for the created user */
        var authenticationCredentials = new AuthenticationCredentials
        {
            Username = credentials.Username,
            Password = credentials.Password
        };

        /* act: attempt to authenticate with the newly created identity */
        var authenticationResult = await identityClient.AuthenticateAsync(authenticationCredentials);

        /* assert: ensure the authentication was successful */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        /* assert: verify that both access and refresh tokens are returned and not empty */
        Assert.NotEmpty(authenticationResult.Data.AccessToken);
        Assert.NotEmpty(authenticationResult.Data.RefreshToken);
    }

    [Fact(DisplayName = "[e2e] - when create identity with an existing username should fail with conflict error")]
    public async Task WhenCreateIdentity_WithExistingUsername_ShouldFailWithConflict()
    {
        /* arrange: create an identity client with the proper realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));

        /* arrange: define fixed enrollment credentials for the user */
        var credentials = new IdentityEnrollmentCredentials
        {
            Username = $"user_{Guid.NewGuid():N}",
            Password = "SuperSecret123!"
        };

        /* act: first creation should succeed */
        var firstCreationResult = await identityClient.CreateIdentityAsync(credentials);

        /* act: second creation with the same username */
        var secondCreationResult = await identityClient.CreateIdentityAsync(credentials);

        /* assert: ensure the second creation failed */
        Assert.True(firstCreationResult.IsSuccess);
        Assert.False(secondCreationResult.IsSuccess);

        Assert.NotNull(secondCreationResult.Error);

        /* assert: verify the correct conflict error was returned */
        Assert.Equal(IdentityErrors.UserAlreadyExists.Code, secondCreationResult.Error.Code);
        Assert.Equal(IdentityErrors.UserAlreadyExists.Description, secondCreationResult.Error.Description);
    }

    [Fact(DisplayName = "[e2e] - when invalidate session for admin should succeed")]
    public async Task WhenInvalidateSession_ForAdmin_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* arrange: authenticate with admin to obtain a valid session */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        /* arrange: prepare session invalidation using admin's refresh token */
        var sessionInvalidation = new SessionInvalidation
        {
            RefreshToken = authenticationResult.Data.RefreshToken
        };

        /* act: invalidate the session */
        var invalidationResult = await identityClient.InvalidateSessionAsync(sessionInvalidation);

        /* assert: ensure the invalidation was successful */
        Assert.True(invalidationResult.IsSuccess);
    }
}