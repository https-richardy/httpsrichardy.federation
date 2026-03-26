namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class ListRealmAssignedPermissionsHandler(IRealmCollection collection) :
    IDispatchHandler<ListRealmAssignedPermissionsParameters, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        ListRealmAssignedPermissionsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await collection.GetRealmsAsync(filters, cancellation);
        var realm = realms.FirstOrDefault();

        return realm is not null
            ? Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(realm.Permissions))
            : Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(RealmErrors.RealmDoesNotExist);
    }
}
