namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class ScopesEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    private readonly Fixture _fixture = new();

    [Fact(DisplayName = "[e2e] - when POST /scopes with valid data should create a new scope successfully")]
    public async Task WhenPostScopesWithValidData_ShouldCreateScopeSuccessfully()
    {
        /* arrange: prepare credentials and authenticate to get access token */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        /* act: send POST request to authenticate endpoint */
        var response = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.NotNull(authentication);
        Assert.NotNull(authentication.AccessToken);

        httpClient.WithAuthorization(authentication.AccessToken);

        /* arrange: build payload for scope creation */
        var payload = _fixture.Build<ScopeCreationScheme>()
            .With(scope => scope.Name, "federation.scopes.orders")
            .Create();

        var httpResponse = await httpClient.PostAsJsonAsync("api/v1/scopes", payload);
        var content = await httpResponse.Content.ReadFromJsonAsync<ScopeDetailsScheme>();

        /* assert: ensure the response and content are not null */
        Assert.NotNull(httpResponse);
        Assert.NotNull(content);

        /* assert: verify the response status code is 201 */
        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        /* assert: verify the returned content matches the sent payload */
        Assert.Equal(payload.Name, content.Name);
        Assert.Equal(payload.Description, content.Description);
    }

    [Fact(DisplayName = "[e2e] - when POST /scopes with duplicate name should return 409 Conflict")]
    public async Task WhenPostScopesWithDuplicateName_ShouldReturnConflict()
    {
        /* arrange: prepare httpClient and authenticate */
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var credentials = new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        };

        /* act: send POST request to authenticate endpoint */
        var response = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials);
        var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

        /* assert: ensure authentication succeeded */
        Assert.NotNull(authentication);
        Assert.NotNull(authentication.AccessToken);

        httpClient.WithAuthorization(authentication.AccessToken);

        /* arrange: build payload with a fixed scope name to simulate duplicate */
        var payload = _fixture.Build<ScopeCreationScheme>()
            .With(scope => scope.Name, "federation.scopes.photos")
            .Create();

        /* act: send first POST request to create the scope (ensure it exists) */
        var httpResponse = await httpClient.PostAsJsonAsync("api/v1/scopes", payload);

        Assert.Equal(HttpStatusCode.Created, httpResponse.StatusCode);

        /* act: send second POST request with the same scope name to trigger conflict */
        var conflictResponse = await httpClient.PostAsJsonAsync("api/v1/scopes", payload);

        /* assert: ensure the response is 409 Conflict */
        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
    }
}
