namespace HttpsRichardy.Federation.Application.Handlers.Secret;

public sealed class FetchRealmSecretsHandler(ISecretCollection collection) :
    IDispatchHandler<FetchRealmSecretsParameters, Result<IReadOnlyCollection<SecretScheme>>>
{
    public async Task<Result<IReadOnlyCollection<SecretScheme>>> HandleAsync(
        FetchRealmSecretsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(parameters.RealmId)
            .Build();

        var secrets = await collection.GetSecretsAsync(filters, cancellation);
        var schemes = secrets.Select(secret => secret.AsResponse())
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyCollection<SecretScheme>>.Success(schemes);
    }
}
