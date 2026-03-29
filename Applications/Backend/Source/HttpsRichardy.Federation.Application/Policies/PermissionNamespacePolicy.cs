namespace HttpsRichardy.Federation.Application.Policies;

public sealed class PermissionNamespacePolicy : IPermissionNamespacePolicy
{
    public Task<Result> EnsurePermissionIsAllowedAsync(
        Realm realm, Permission permission, CancellationToken cancellation = default)
    {
        var isReserved = RealmPermissions.SystemPermissions
            .Contains(permission.Name);

        return isReserved
            ? Task.FromResult(Result.Failure(PermissionErrors.PermissionNameIsReserved))
            : Task.FromResult(Result.Success());
    }
}
