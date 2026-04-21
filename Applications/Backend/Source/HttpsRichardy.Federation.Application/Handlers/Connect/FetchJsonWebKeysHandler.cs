namespace HttpsRichardy.Federation.Application.Handlers.Connect;

public sealed class FetchJsonWebKeysHandler(ISecretCollection collection, IRealmCollection realmCollection) :
    IDispatchHandler<FetchJsonWebKeysParameters, Result<JsonWebKeySetScheme>>
{
    public async Task<Result<JsonWebKeySetScheme>> HandleAsync(
        FetchJsonWebKeysParameters parameters, CancellationToken cancellation = default)
    {
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName(parameters.Realm)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<JsonWebKeySetScheme>.Failure(RealmErrors.RealmDoesNotExist);
        }

        var filters = SecretFilters.WithSpecifications()
            .WithCanValidate()
            .WithRealm(realm.Id)
            .Build();

        var secrets = await collection.GetSecretsAsync(filters, cancellation);
        var jwks = JsonWebKeysMapper.AsJsonWebKeySetScheme(secrets);

        return Result<JsonWebKeySetScheme>.Success(jwks);
    }
}
