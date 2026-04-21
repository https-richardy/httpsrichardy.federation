namespace HttpsRichardy.Federation.Domain.Collections;

public interface ISecretCollection : IAggregateCollection<Secret>
{
    public Task<IReadOnlyCollection<Secret>> GetSecretsAsync(
        SecretFilters filters,
        CancellationToken cancellation = default
    );

    public Task<System.Numerics.BigInteger> CountSecretsAsync(
        SecretFilters filters,
        CancellationToken cancellation = default
    );
}
