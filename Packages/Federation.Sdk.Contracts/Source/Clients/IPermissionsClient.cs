namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IPermissionsClient
{
    public Task<Result<Pagination<PermissionDetails>>> GetPermissionsAsync(
        PermissionsFetchParameters parameters,
        CancellationToken cancellation = default
    );

    public Task<Result<PermissionDetails>> CreatePermissionAsync(
        PermissionCreationScheme permission,
        CancellationToken cancellation = default
    );
    public Task<Result<PermissionDetails>> UpdatePermissionAsync(
        PermissionUpdateScheme permission,
        CancellationToken cancellation = default
    );

    public Task<Result> DeletePermissionAsync(
        string permissionId,
        CancellationToken cancellation = default
    );
}