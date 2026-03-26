namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class RealmUpdateHandler(IRealmCollection collection) :
    IDispatchHandler<RealmUpdateScheme, Result<RealmDetailsScheme>>
{
    public async Task<Result<RealmDetailsScheme>> HandleAsync(RealmUpdateScheme parameters, CancellationToken cancellation = default)
    {
        var filters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await collection.GetRealmsAsync(filters, cancellation: cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<RealmDetailsScheme>.Failure(RealmErrors.RealmDoesNotExist);
        }

        realm = RealmMapper.AsRealm(parameters, realm);

        var updatedRealm = await collection.UpdateAsync(realm, cancellation: cancellation);

        return Result<RealmDetailsScheme>.Success(RealmMapper.AsResponse(updatedRealm));
    }
}
