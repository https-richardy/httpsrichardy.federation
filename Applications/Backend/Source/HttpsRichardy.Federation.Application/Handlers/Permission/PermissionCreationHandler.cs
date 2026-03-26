namespace HttpsRichardy.Federation.Application.Handlers.Permission;

public sealed class PermissionCreationHandler(IPermissionCollection collection, IPermissionNamespacePolicy policy, IRealmProvider realmProvider) :
    IDispatchHandler<PermissionCreationScheme, Result<PermissionDetailsScheme>>
{
    public async Task<Result<PermissionDetailsScheme>> HandleAsync(PermissionCreationScheme parameters, CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var result = await policy.EnsurePermissionIsAllowedAsync(realm, new() { Name = parameters.Name }, cancellation);

        if (result.IsFailure)
        {
            return Result<PermissionDetailsScheme>.Failure(PermissionErrors.PermissionNameIsReserved);
        }

        var filters = PermissionFilters.WithSpecifications()
            .WithName(parameters.Name)
            .Build();

        var permissions = await collection.GetPermissionsAsync(filters, cancellation: cancellation);
        var existingPermission = permissions.FirstOrDefault();

        if (existingPermission is not null)
        {
            return Result<PermissionDetailsScheme>.Failure(PermissionErrors.PermissionAlreadyExists);
        }

        var permission = PermissionMapper.AsPermission(parameters, realm);
        var createdPermission = await collection.InsertAsync(permission, cancellation: cancellation);

        return Result<PermissionDetailsScheme>.Success(PermissionMapper.AsResponse(createdPermission));
    }
}
