namespace HttpsRichardy.Federation.TestSuite.Integration.Endpoints;

public sealed class WellKnownEndpointTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should return OpenID configuration")]
    public async Task WhenGetOpenIdConfiguration_ShouldReturnConfiguration()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to open id configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: configuration should contain required open id connect fields */

        Assert.False(string.IsNullOrWhiteSpace(configuration.Issuer));
        Assert.False(string.IsNullOrWhiteSpace(configuration.AuthorizationEndpoint));
        Assert.False(string.IsNullOrWhiteSpace(configuration.TokenEndpoint));
        Assert.False(string.IsNullOrWhiteSpace(configuration.UserInfoEndpoint));
        Assert.False(string.IsNullOrWhiteSpace(configuration.JwksUri));
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should include supported response types")]
    public async Task WhenGetOpenIdConfiguration_ShouldIncludeSupportedResponseTypes()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to OpenID configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: configuration should contain response_types_supported */
        Assert.NotNull(configuration.ResponseTypesSupported);
        Assert.NotEmpty(configuration.ResponseTypesSupported);
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should include subject types")]
    public async Task WhenGetOpenIdConfiguration_ShouldIncludeSubjectTypes()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to OpenID configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: configuration should contain subject_types_supported */
        Assert.NotNull(configuration.SubjectTypesSupported);
        Assert.NotEmpty(configuration.SubjectTypesSupported);
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should include signing algorithms")]
    public async Task WhenGetOpenIdConfiguration_ShouldIncludeSigningAlgorithms()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to OpenID configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: configuration should contain id_token_signing_alg_values_supported */
        Assert.NotNull(configuration.IdTokenSigningAlgValuesSupported);
        Assert.NotEmpty(configuration.IdTokenSigningAlgValuesSupported);
        Assert.Contains("RS256", configuration.IdTokenSigningAlgValuesSupported);
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/jwks.json should return JSON Web Keys")]
    public async Task WhenGetJsonWebKeys_ShouldReturnJwks()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to JWKS endpoint */
        var response = await httpClient.GetAsync(".well-known/jwks.json");
        var jwks = await response.Content.ReadFromJsonAsync<JsonWebKeySetScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(jwks);

        /* assert: JWKS should contain keys array */
        Assert.NotNull(jwks.Keys);
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/jwks.json should return keys with required properties")]
    public async Task WhenGetJsonWebKeys_ShouldReturnKeysWithRequiredProperties()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to JWKS endpoint */
        var response = await httpClient.GetAsync(".well-known/jwks.json");
        var jwks = await response.Content.ReadFromJsonAsync<JsonWebKeySetScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull(jwks);
        Assert.NotNull(jwks.Keys);

        /* assert: if keys exist, they should have required properties */
        if (jwks.Keys.Any())
        {
            var firstKey = jwks.Keys.First();

            /* assert: key should have required properties */
            Assert.False(string.IsNullOrWhiteSpace(firstKey.Type));
            Assert.Equal("RSA", firstKey.Type);

            Assert.False(string.IsNullOrWhiteSpace(firstKey.Use));
            Assert.Equal("sig", firstKey.Use);

            Assert.False(string.IsNullOrWhiteSpace(firstKey.Identifier));
            Assert.False(string.IsNullOrWhiteSpace(firstKey.Exponent));
            Assert.False(string.IsNullOrWhiteSpace(firstKey.Modulus));
        }
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/jwks.json multiple times should return consistent keys")]
    public async Task WhenGetJsonWebKeysMultipleTimes_ShouldReturnConsistentKeys()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request twice to JWKS endpoint */
        var firstResponse = await httpClient.GetAsync(".well-known/jwks.json");
        var firstJwks = await firstResponse.Content.ReadFromJsonAsync<JsonWebKeySetScheme>();

        var secondResponse = await httpClient.GetAsync(".well-known/jwks.json");
        var secondJwks = await secondResponse.Content.ReadFromJsonAsync<JsonWebKeySetScheme>();

        /* assert: both responses should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        /* assert: both responses should have the same structure */
        Assert.NotNull(firstJwks);
        Assert.NotNull(secondJwks);
        Assert.NotNull(firstJwks.Keys);
        Assert.NotNull(secondJwks.Keys);
        Assert.Equal(firstJwks.Keys.Count(), secondJwks.Keys.Count());
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should have matching jwks_uri")]
    public async Task WhenGetOpenIdConfiguration_ShouldHaveMatchingJwksUri()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to OpenID configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: jwks_uri should be accessible */
        Assert.False(string.IsNullOrWhiteSpace(configuration.JwksUri));

        /* act: verify jwks_uri is accessible */
        var jwksResponse = await httpClient.GetAsync(new Uri(configuration.JwksUri).PathAndQuery);

        /* assert: jwks endpoint should be accessible */
        Assert.Equal(HttpStatusCode.OK, jwksResponse.StatusCode);
    }

    [Fact(DisplayName = "[e2e] - when GET /.well-known/openid-configuration should have valid endpoint URLs")]
    public async Task WhenGetOpenIdConfiguration_ShouldHaveValidEndpointUrls()
    {
        /* arrange: prepare request */
        var httpClient = factory.HttpClient;

        /* act: send GET request to OpenID configuration endpoint */
        var response = await httpClient.GetAsync(".well-known/openid-configuration");
        var configuration = await response.Content.ReadFromJsonAsync<OpenIDConfigurationScheme>();

        /* assert: response should be 200 OK */
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(configuration);

        /* assert: all endpoint URLs should be valid URIs */
        Assert.True(Uri.TryCreate(configuration.Issuer, UriKind.Absolute, out _));
        Assert.True(Uri.TryCreate(configuration.AuthorizationEndpoint, UriKind.Absolute, out _));
        Assert.True(Uri.TryCreate(configuration.TokenEndpoint, UriKind.Absolute, out _));
        Assert.True(Uri.TryCreate(configuration.UserInfoEndpoint, UriKind.Absolute, out _));
        Assert.True(Uri.TryCreate(configuration.JwksUri, UriKind.Absolute, out _));
    }
}
