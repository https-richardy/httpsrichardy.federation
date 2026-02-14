namespace HttpsRichardy.Federation.Application.Handlers.Permission;

public sealed class PermissionUpdateHandler(IPermissionCollection collection, IRealmProvider realmProvider) :
    IDispatchHandler<PermissionUpdateScheme, Result<PermissionDetailsScheme>>
{
    public async Task<Result<PermissionDetailsScheme>> HandleAsync(PermissionUpdateScheme parameters, CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var filters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var permissions = await collection.GetPermissionsAsync(filters, cancellation: cancellation);
        var permission = permissions.FirstOrDefault();

        if (permission is null)
        {
            return Result<PermissionDetailsScheme>.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        permission = PermissionMapper.AsPermission(parameters, permission, realm);

        var updatedPermission = await collection.UpdateAsync(permission, cancellation: cancellation);

        return Result<PermissionDetailsScheme>.Success(PermissionMapper.AsResponse(updatedPermission));
    }
}