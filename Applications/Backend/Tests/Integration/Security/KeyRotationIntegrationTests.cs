using System.Text.Json;

namespace HttpsRichardy.Federation.TestSuite.Integration.Security;

public sealed class KeyRotationIntegrationTests(IntegrationEnvironmentFixture factory) :
    IClassFixture<IntegrationEnvironmentFixture>
{
    [Fact(DisplayName = "[e2e] - when rotating keys should publish new kid on realm jwks")]
    public async Task WhenRotateKeys_ShouldPublishNewKidOnRealmJwks()
    {
        var httpClient = factory.HttpClient.WithRealmHeader("master");
        var rotationService = factory.Services.GetRequiredService<ISecretRotationService>();

        var authBefore = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        });

        var authentication = await authBefore.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authentication);
        Assert.False(string.IsNullOrWhiteSpace(authentication.AccessToken));

        var handlerBefore = new JwtSecurityTokenHandler();

        var jwtBefore = handlerBefore.ReadJwtToken(authentication.AccessToken);
        var kidBefore = jwtBefore.Header.Kid;

        Assert.False(string.IsNullOrWhiteSpace(kidBefore));

        var realmCollection = factory.Services.GetRequiredService<IRealmCollection>();
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, CancellationToken.None);
        var realm = realms.FirstOrDefault();

        Assert.NotNull(realm);

        await rotationService.RotateSecretAsync(realm, CancellationToken.None);

        string? kidAfter = null;

        var started = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(10))
        {
            var authAfter = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
            {
                Username = "federation.testing.user",
                Password = "federation.testing.password"
            });

            var authenticationResult = await authAfter.Content.ReadFromJsonAsync<AuthenticationResult>();

            Assert.NotNull(authenticationResult);

            var handlerAfter = new JwtSecurityTokenHandler();
            var jwtAfter = handlerAfter.ReadJwtToken(authenticationResult.AccessToken);

            kidAfter = jwtAfter.Header.Kid;

            if (!string.Equals(kidAfter, kidBefore, StringComparison.Ordinal))
                break;

            await Task.Delay(300);
        }

        Assert.False(string.IsNullOrWhiteSpace(kidAfter));
        Assert.NotEqual(kidBefore, kidAfter);

        var jwksResponse = await httpClient.GetAsync("master/.well-known/jwks.json");
        var jwksRaw = await jwksResponse.Content.ReadAsStringAsync();

        using var jwks = JsonDocument.Parse(jwksRaw);

        Assert.True(jwks.RootElement.TryGetProperty("keys", out var keys));
        Assert.Equal(JsonValueKind.Array, keys.ValueKind);

        var jwksKids = keys.EnumerateArray()
            .Where(key => key.TryGetProperty("kid", out _))
            .Select(key => key.GetProperty("kid").GetString())
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Cast<string>()
            .ToArray();

        Assert.Contains(kidAfter!, jwksKids);
    }

    [Fact(DisplayName = "[e2e] - when rotating keys should keep old token valid while old key is published")]
    public async Task WhenRotateKeys_ShouldKeepOldTokenValidWhileOldKeyIsPublished()
    {
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var rotationService = factory.Services.GetRequiredService<ISecretRotationService>();
        var realmCollection = factory.Services.GetRequiredService<IRealmCollection>();

        var authBefore = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
        {
            Username = "federation.testing.user",
            Password = "federation.testing.password"
        });

        var authentication = await authBefore.Content.ReadFromJsonAsync<AuthenticationResult>();

        Assert.NotNull(authentication);
        Assert.NotEmpty(authentication.AccessToken);

        var oldToken = authentication.AccessToken;
        var oldJwt = new JwtSecurityTokenHandler().ReadJwtToken(oldToken);
        var oldKid = oldJwt.Header.Kid;

        Assert.False(string.IsNullOrWhiteSpace(oldKid));

        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, CancellationToken.None);
        var realm = realms.FirstOrDefault();

        Assert.NotNull(realm);

        await rotationService.RotateSecretAsync(realm, CancellationToken.None);

        string? newKid = null;
        var started = DateTimeOffset.UtcNow;

        while (DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(10))
        {
            var authAfter = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
            {
                Username = "federation.testing.user",
                Password = "federation.testing.password"
            });

            var authPayloadAfter = await authAfter.Content.ReadFromJsonAsync<AuthenticationResult>();

            Assert.NotNull(authPayloadAfter);

            var jwtAfter = new JwtSecurityTokenHandler().ReadJwtToken(authPayloadAfter.AccessToken);

            newKid = jwtAfter.Header.Kid;

            if (!string.Equals(oldKid, newKid, StringComparison.Ordinal))
                break;

            await Task.Delay(300);
        }

        Assert.False(string.IsNullOrWhiteSpace(newKid));

        var jwksResponse = await httpClient.GetAsync("master/.well-known/jwks.json");
        var jwksRaw = await jwksResponse.Content.ReadAsStringAsync();

        using var jwks = JsonDocument.Parse(jwksRaw);

        Assert.True(jwks.RootElement.TryGetProperty("keys", out var keys));

        var signingKeys = keys.EnumerateArray()
            .Select(key => new JsonWebKey(key.GetRawText()) as SecurityKey)
            .ToArray();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = signingKeys,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireSignedTokens = true
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationSucceeded = true;

        try
        {
            tokenHandler.ValidateToken(oldToken, validationParameters, out _);
        }
        catch
        {
            validationSucceeded = false;
        }

        Assert.True(validationSucceeded);
    }
}
