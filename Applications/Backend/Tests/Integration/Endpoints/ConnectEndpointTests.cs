namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class ConnectEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with valid client credentials should return access token")]
    public async Task WhenPostTokenWithValidClientCredentials_ShouldReturnAccessToken()
    {
        /* arrange: authenticate user and get access token */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var userCredentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", userCredentials);
        var authentication = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authentication);
        Assert.NotEmpty(authentication.AccessToken);

        httpClient.WithAuthorization(authentication.AccessToken);

        /* arrange: create a client */
        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, "root")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var response = await httpClient.PostAsJsonAsync("api/v1/clients", payload);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var filters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: prepare client credentials */
        var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", client.ClientId },
            { "client_secret", client.Secret }
        };

        var content = new FormUrlEncodedContent(credentials);
        var connectClient = factory.HttpClient;

        /* act: send POST request to token endpoint */
        var httpResponse = await connectClient.PostAsync("api/v1/protocol/open-id/connect/token", content);
        var grantedToken = await httpResponse.Content.ReadFromJsonAsync<ClientAuthenticationResult>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);

        Assert.NotNull(grantedToken);
        Assert.False(string.IsNullOrWhiteSpace(grantedToken.AccessToken));
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with non-existent client should return 401 #ERROR-0AF50")]
    public async Task WhenPostTokenWithNonExistentClient_ShouldReturnUnauthorized()
    {
        /* arrange: prepare credentials with non-existent client */
        var httpClient = factory.HttpClient;
        var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", $"non-existent-client-{Guid.NewGuid()}" },
            { "client_secret", "some-secret" }
        };

        var content = new FormUrlEncodedContent(credentials);

        /* act: send POST request with non-existent client */
        var response = await httpClient.PostAsync("api/v1/protocol/open-id/connect/token", content);
        var error = await response.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 401 Unauthorized */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(AuthenticationErrors.ClientNotFound, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with invalid client secret should return 401 #ERROR-A7E7C")]
    public async Task WhenPostTokenWithInvalidClientSecret_ShouldReturnUnauthorized()
    {
        /* arrange: authenticate user and get access token */
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var userCredentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authenticationResponse = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", userCredentials);
        var authentication = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authentication);

        httpClient.WithAuthorization(authentication.AccessToken);

        /* arrange: create a client */
        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, "admin")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var response = await httpClient.PostAsJsonAsync("api/v1/clients", payload);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var filters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        /* arrange: prepare credentials with wrong secret */
        var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", client.ClientId },
            { "client_secret", "wrong-secret" }
        };

        /* act: send POST request with invalid secret */
        var content = new FormUrlEncodedContent(credentials);
        var connectClient = factory.HttpClient;

        var httpResponse = await connectClient.PostAsync("api/v1/protocol/open-id/connect/token", content);
        var error = await httpResponse.Content.ReadFromJsonAsync<Error>();

        /* assert: response should be 401 Unauthorized */
        Assert.NotNull(error);

        Assert.Equal(HttpStatusCode.Unauthorized, httpResponse.StatusCode);
        Assert.Equal(AuthenticationErrors.InvalidClientCredentials, error);
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with missing grant_type should return 400")]
    public async Task WhenPostTokenWithMissingGrantType_ShouldReturnBadRequest()
    {
        /* arrange: prepare credentials without grant_type */
        var httpClient = factory.HttpClient;
        var credentials = new Dictionary<string, string>
        {
            { "client_id", "test-client-id" },
            { "client_secret", "test-client-secret" }
        };

        /* act: send POST request without grant_type */

        var content = new FormUrlEncodedContent(credentials);
        var response = await httpClient.PostAsync("api/v1/protocol/open-id/connect/token", content);

        /* assert: response should be 400 Bad Request */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with missing client_id should return 400")]
    public async Task WhenPostTokenWithMissingClientId_ShouldReturnBadRequest()
    {
        /* arrange: prepare credentials without client_id */
        var httpClient = factory.HttpClient;
        var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_secret", "test-client-secret" }
        };

        /* act: send POST request without client_id */

        var content = new FormUrlEncodedContent(credentials);
        var response = await httpClient.PostAsync("api/v1/protocol/open-id/connect/token", content);

        /* assert: response should be 400 Bad Request */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with missing client_secret should return 400")]
    public async Task WhenPostTokenWithMissingClientSecret_ShouldReturnBadRequest()
    {
        /* arrange: prepare credentials without client_secret */
        var httpClient = factory.HttpClient;
        var credentials = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", "test-client-id" }
        };

        /* act: send POST request without client_secret */

        var content = new FormUrlEncodedContent(credentials);
        var response = await httpClient.PostAsync("api/v1/protocol/open-id/connect/token", content);

        /* assert: response should be 400 Bad Request */
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when POST /openid/connect/token with valid authorization_code should return access token")]
    public async Task WhenPostTokenWithValidAuthorizationCode_ShouldReturnAccessToken()
    {
        // arrange: resolve required dependencies
        var tokenCollection = factory.Services.GetRequiredService<ITokenCollection>();
        var userCollection = factory.Services.GetRequiredService<IUserCollection>();
        var clientCollection = factory.Services.GetRequiredService<IClientCollection>();

        // arrange: authenticate as master to create client and realm
        var masterClient = factory.HttpClient.WithRealmHeader("master");
        var masterCredentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        var authentication = await masterClient.PostAsJsonAsync("api/v1/identity/authenticate", masterCredentials);
        var grantedToken = await authentication.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(grantedToken);
        Assert.NotEmpty(grantedToken.AccessToken);

        masterClient.WithAuthorization(grantedToken.AccessToken);

        // arrange: create realm
        var realmPayload = _fixture.Build<RealmCreationScheme>()
            .With(realm => realm.Name, $"test-realm-{Guid.NewGuid()}")
            .With(realm => realm.Description, $"test-description-{Guid.NewGuid()}")
            .Create();

        var realmResponse = await masterClient.PostAsJsonAsync("api/v1/realms", realmPayload);
        var realm = await realmResponse.Content.ReadFromJsonAsync<RealmDetailsScheme>();

        Assert.NotNull(realm);
        Assert.Equal(HttpStatusCode.Created, realmResponse.StatusCode);

        // arrange: create client for authorization_code grant
        var realmMasterClient = factory.HttpClient.WithRealmHeader(realm.Name);
        var realmAuth = await realmMasterClient.PostAsJsonAsync("api/v1/identity/authenticate", masterCredentials);

        var realmAdminClient = factory.HttpClient
            .WithRealmHeader(realm.Name)
            .WithAuthorization(grantedToken.AccessToken);

        var payload = _fixture.Build<ClientCreationScheme>()
            .With(client => client.Name, "root")
            .With(client => client.Flows, [Grant.ClientCredentials])
            .With(client => client.RedirectUris, [])
            .Create();

        var httpResponse = await realmAdminClient.PostAsJsonAsync("api/v1/clients", payload);

        Assert.NotNull(httpResponse);
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        var clientFilters = ClientFilters.WithSpecifications()
            .WithName(payload.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters);
        var client = clients.FirstOrDefault();

        Assert.NotEmpty(clients);
        Assert.NotNull(client);

        // arrange: create user for realm
        var credentials = new IdentityEnrollmentCredentials
        {
            Username = $"user.{Guid.NewGuid()}@email.com",
            Password = "TestPassword123!"
        };

        var realmClient = factory.HttpClient.WithRealmHeader(realm.Name);

        var enrollment = await realmClient.PostAsJsonAsync("api/v1/identity", credentials);
        var identity = await enrollment.Content.ReadFromJsonAsync<UserDetailsScheme>();

        Assert.NotNull(identity);
        Assert.Equal(HttpStatusCode.Created, enrollment.StatusCode);

        // arrange: authenticate new user
        var authenticationCredentials = new AuthenticationCredentials
        {
            Username = credentials.Username,
            Password = credentials.Password
        };

        var authenticationResponse = await realmClient.PostAsJsonAsync("api/v1/identity/authenticate", authenticationCredentials);
        var authenticationResult = await authenticationResponse.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authenticationResult);
        Assert.NotEmpty(authenticationResult.AccessToken);

        realmClient.WithAuthorization(authenticationResult.AccessToken);

        // arrange: generate PKCE
        var codeVerifier = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var codeChallenge = Application.Utilities.Base64UrlEncoder.Encode(SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier)));
        var codeChallengeMethod = "S256";

        // arrange: get user from db
        var filters = UserFilters.WithSpecifications()
            .WithUsername(credentials.Username)
            .Build();

        var users = await userCollection.GetUsersAsync(filters);
        var user = users.FirstOrDefault();

        Assert.NotEmpty(users);
        Assert.NotNull(user);

        // arrange: create authorization code token
        var authorizationCode = Guid.NewGuid().ToString("N");
        var token = new Domain.Aggregates.SecurityToken
        {
            Value = authorizationCode,
            UserId = user.Id,
            RealmId = realm.Id,
            Type = TokenType.AuthorizationCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            Metadata = new Dictionary<string, string>
            {
                ["code.challenge"] = codeChallenge,
                ["code.challenge.method"] = codeChallengeMethod
            }
        };

        await tokenCollection.InsertAsync(token);

        // arrange: prepare authorization_code grant request
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "client_id", client.ClientId },
            { "code_verifier", codeVerifier }
        };

        var content = new FormUrlEncodedContent(parameters);
        var connectClient = factory.HttpClient.WithRealmHeader(realm.Name);

        // act: send POST request to token endpoint
        var response = await connectClient.PostAsync("api/v1/protocol/open-id/connect/token", content);
        var grant = await response.Content.ReadFromJsonAsync<ClientAuthenticationResult>();

        // assert: response should be 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(grant);
    }
}
