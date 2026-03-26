namespace HttpsRichardy.Federation.Application.Policies;

public sealed class PermissionNamespacePolicy : IPermissionNamespacePolicy
{
    public async Task<Result> EnsurePermissionIsAllowedAsync(
        Realm realm, Permission permission, CancellationToken cancellation = default)
    {
        var isReserved = RealmPermissions.SystemPermissions
            .Contains(permission.Name);

        return isReserved
            ? Result.Failure(PermissionErrors.PermissionNameIsReserved)
            : Result.Success();
    }
}
