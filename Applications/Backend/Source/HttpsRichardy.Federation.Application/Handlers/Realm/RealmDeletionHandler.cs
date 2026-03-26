namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class RealmDeletionHandler(IRealmCollection collection) : IDispatchHandler<RealmDeletionScheme, Result>
{
    public async Task<Result> HandleAsync(RealmDeletionScheme parameters, CancellationToken cancellation = default)
    {
        var filters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await collection.GetRealmsAsync(filters, cancellation: cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result.Failure(RealmErrors.RealmDoesNotExist);
        }

        await collection.DeleteAsync(realm, cancellation: cancellation);

        return Result.Success();
    }
}
