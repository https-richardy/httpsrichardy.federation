namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class AssignPermissionToRealmHandler(IRealmCollection realmCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<AssignRealmPermissionScheme, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        AssignRealmPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var realmFilters = new RealmFiltersBuilder()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var permissionFilters = new PermissionFiltersBuilder()
            .WithName(parameters.PermissionName.ToLower())
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation: cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(RealmErrors.RealmDoesNotExist);
        }

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation: cancellation);
        var existingPermission = permissions.FirstOrDefault();

        if (existingPermission is null)
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        if (realm.Permissions.Any(permission => permission.Name == existingPermission.Name))
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(RealmErrors.RealmAlreadyHasPermission);
        }

        realm.Permissions.Add(existingPermission);

        await realmCollection.UpdateAsync(realm, cancellation: cancellation);

        return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(realm.Permissions));
    }
}
