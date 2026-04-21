using HttpsRichardy.Internal.Essentials.Contracts.Behaviors;

using Realm = HttpsRichardy.Federation.Domain.Aggregates.Realm;
using Secret = HttpsRichardy.Federation.Domain.Aggregates.Secret;

namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class SecretRotationService(ISecretCollection secretCollection) : ISecretRotationService
{
    private static readonly TimeSpan _keyLifetime = TimeSpan.FromDays(30);
    private static readonly TimeSpan _gracePeriod = TimeSpan.FromDays(1);

    public async Task CreateSecretAsync(Realm realm, CancellationToken cancellation = default)
    {
        using var rsa = RSA.Create(2048);

        var now = DateTime.UtcNow;
        var secret = new Secret
        {
            RealmId = realm.Id,

            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),

            CreatedAt = now,
            ExpiresAt = now.Add(_keyLifetime),
        };

        await secretCollection.InsertAsync(secret, cancellation: cancellation);
    }

    public async Task DeleteSecretAsync(Realm realm, Secret secret, CancellationToken cancellation = default)
    {
        if (secret.GracePeriodEndsAt is not null && secret.GracePeriodEndsAt <= DateTime.UtcNow)
        {
            await secretCollection.DeleteAsync(secret, behavior: DeletionBehavior.Hard, cancellation: cancellation);
        }
    }

    public async Task EnsureSecretExistsAsync(Realm realm, CancellationToken cancellation = default)
    {
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .WithCanSign()
            .Build();

        var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);
        var current = secrets
            .OrderByDescending(secret => secret.CreatedAt)
            .FirstOrDefault();

        if (current is null)
        {
            await CreateSecretAsync(realm, cancellation);
        }
    }

    public async Task PruneSecretsAsync(Realm realm, CancellationToken cancellation = default)
    {
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .Build();

        var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);

        foreach (var secret in secrets)
        {
            await DeleteSecretAsync(realm, secret, cancellation);
        }
    }

    public async Task RotateSecretAsync(Realm realm, CancellationToken cancellation = default)
    {
        var now = DateTime.UtcNow;
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .WithCanSign(now)
            .Build();

        var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);
        var current = secrets
            .OrderByDescending(secret => secret.CreatedAt)
            .FirstOrDefault();

        if (current is null)
        {
            await CreateSecretAsync(realm, cancellation);
            return;
        }

        if (current.ExpiresAt is not null && current.ExpiresAt > now)
            return;

        current.ExpiresAt = now;
        current.GracePeriodEndsAt = now.Add(_gracePeriod);

        await secretCollection.UpdateAsync(current, cancellation: cancellation);
        await CreateSecretAsync(realm, cancellation);
    }
}
