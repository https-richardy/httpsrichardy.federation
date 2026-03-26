namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class RevokeRealmPermissionHandler(IRealmCollection realmCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<RevokeRealmPermissionScheme, Result>
{
    public async Task<Result> HandleAsync(RevokeRealmPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var realmFilters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation);
        var realm = realms.FirstOrDefault();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation);
        var permission = permissions.FirstOrDefault();

        if (realm is null)
        {
            return Result.Failure(RealmErrors.RealmDoesNotExist);
        }

        if (permission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        var permissionToRemove = realm.Permissions.FirstOrDefault(p => p.Id == permission.Id);
        if (permissionToRemove is null)
        {
            return Result.Failure(RealmErrors.PermissionNotAssigned);
        }

        realm.Permissions.Remove(permissionToRemove);

        await realmCollection.UpdateAsync(realm, cancellation);

        return Result.Success();
    }
}
