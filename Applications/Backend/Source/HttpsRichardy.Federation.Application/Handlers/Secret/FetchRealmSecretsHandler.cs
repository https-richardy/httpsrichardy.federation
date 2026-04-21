namespace HttpsRichardy.Federation.Application.Handlers.Secret;

public sealed class FetchRealmSecretsHandler(ISecretCollection collection, IRealmCollection realmCollection) :
    IDispatchHandler<FetchRealmSecretsParameters, Result<IReadOnlyCollection<SecretScheme>>>
{
    public async Task<Result<IReadOnlyCollection<SecretScheme>>> HandleAsync(
        FetchRealmSecretsParameters parameters, CancellationToken cancellation = default)
    {
        var realmFilters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<IReadOnlyCollection<SecretScheme>>.Failure(RealmErrors.RealmDoesNotExist);
        }

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
