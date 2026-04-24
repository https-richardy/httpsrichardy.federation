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

    [Fact(DisplayName = "[e2e] - when rotating keys jwks should contain multiple keys during grace period")]
    public async Task WhenRotateKeys_JwksShouldContainMultipleKeysDuringGracePeriod()
    {
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var rotationService = factory.Services.GetRequiredService<ISecretRotationService>();
        var realmCollection = factory.Services.GetRequiredService<IRealmCollection>();

        var jwksResponseBefore = await httpClient.GetAsync("master/.well-known/jwks.json");
        var jwksRawBefore = await jwksResponseBefore.Content.ReadAsStringAsync();

        using var jwksBefore = JsonDocument.Parse(jwksRawBefore);

        Assert.NotNull(jwksBefore);
        Assert.True(jwksBefore.RootElement.TryGetProperty("keys", out var keysBefore));

        var oldKidsCount = keysBefore.EnumerateArray()
            .Where(key => key.TryGetProperty("kid", out _))
            .Count();

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
            var response = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
            {
                Username = "federation.testing.user",
                Password = "federation.testing.password"
            });

            var authentication = await response.Content.ReadFromJsonAsync<AuthenticationResult>();

            Assert.NotNull(authentication);
            Assert.NotEmpty(authentication.AccessToken);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(authentication.AccessToken);

            newKid = jwt.Header.Kid;

            var jwksResponse = await httpClient.GetAsync("master/.well-known/jwks.json");
            var jwksRaw = await jwksResponse.Content.ReadAsStringAsync();

            using var jwks = JsonDocument.Parse(jwksRaw);

            Assert.True(jwks.RootElement.TryGetProperty("keys", out var keysAfter));

            var newKidsCount = keysAfter.EnumerateArray()
                .Where(key => key.TryGetProperty("kid", out _))
                .Count();

            if (newKidsCount > oldKidsCount)
            {
                var jwksKids = keysAfter.EnumerateArray()
                    .Where(key => key.TryGetProperty("kid", out _))
                    .Select(key => key.GetProperty("kid").GetString())
                    .Where(key => !string.IsNullOrWhiteSpace(key))
                    .Cast<string>()
                    .ToArray();

                Assert.NotEmpty(jwksKids);
                Assert.True(jwksKids.Length >= 2, "JWKS should contain at least 2 keys during grace period");

                return;
            }

            await Task.Delay(300);
        }

        Assert.Fail("JWKS never showed multiple keys during grace period");
    }

    [Fact(DisplayName = "[e2e] - when grace period expires old key should be removed from database")]
    public async Task WhenGracePeriodExpires_ShouldRemoveOldKeyFromDatabase()
    {
        var httpClient = factory.HttpClient.WithRealmHeader("master");

        var rotationService = factory.Services.GetRequiredService<ISecretRotationService>();
        var realmCollection = factory.Services.GetRequiredService<IRealmCollection>();
        var secretCollection = factory.Services.GetRequiredService<ISecretCollection>();

        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, CancellationToken.None);
        var realm = realms.FirstOrDefault();

        Assert.NotNull(realm);

        var secretFiltersBefore = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .Build();

        var secretsBefore = await secretCollection.GetSecretsAsync(secretFiltersBefore, CancellationToken.None);
        var countBefore = secretsBefore.Count;

        await rotationService.RotateSecretAsync(realm, CancellationToken.None);

        var started = DateTimeOffset.UtcNow;
        while (DateTimeOffset.UtcNow - started < TimeSpan.FromSeconds(10))
        {
            var auth = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", new AuthenticationCredentials
            {
                Username = "federation.testing.user",
                Password = "federation.testing.password"
            });

            var authResult = await auth.Content.ReadFromJsonAsync<AuthenticationResult>();

            Assert.NotNull(authResult);

            var secretsAfterRotation = await secretCollection.GetSecretsAsync(secretFiltersBefore, CancellationToken.None);
            var countAfterRotation = secretsAfterRotation.Count;

            if (countAfterRotation > countBefore)
            {
                var oldSecret = secretsAfterRotation.FirstOrDefault(secret => secret.GracePeriodEndsAt is not null);

                Assert.NotNull(oldSecret);
                Assert.NotNull(oldSecret.GracePeriodEndsAt);

                oldSecret.GracePeriodEndsAt = DateTime.UtcNow.AddSeconds(-1);

                await secretCollection.UpdateAsync(oldSecret, cancellation: CancellationToken.None);
                await rotationService.PruneSecretsAsync(realm, CancellationToken.None);

                var secretsAfterPrune = await secretCollection.GetSecretsAsync(secretFiltersBefore, CancellationToken.None);
                var countAfterPrune = secretsAfterPrune.Count;

                Assert.True(countAfterPrune <= countAfterRotation);

                var removedSecret = secretsAfterPrune.FirstOrDefault(secret => secret.Id == oldSecret.Id);

                Assert.Null(removedSecret);

                return;
            }

            await Task.Delay(300);
        }

        Assert.Fail("Grace period test timeout: rotation did not complete in time");
    }
}
