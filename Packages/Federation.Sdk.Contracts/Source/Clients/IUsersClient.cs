namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IUsersClient
{
    public Task<Result<Pagination<UserDetails>>> GetUsersAsync(
        UsersFetchParameters? parameters = null,
        CancellationToken cancellation = default
    );

    public Task<Result<IReadOnlyCollection<PermissionDetails>>> GetUserPermissionsAsync(
        string userId,
        ListUserAssignedPermissionsParameters? parameters = null,
        CancellationToken cancellation = default
    );

    public Task<Result<IReadOnlyCollection<GroupBasicDetails>>> GetUserGroupsAsync(
        string userId,
        ListUserAssignedGroupsParameters? parameters = null,
        CancellationToken cancellation = default
    );

    public Task<Result> DeleteUserAsync(
        string userId,
        CancellationToken cancellation = default
    );

    public Task<Result> AssignUserGroupAsync(
        string userId,
        string groupId,
        CancellationToken cancellation = default
    );

    public Task<Result> AssignUserPermissionAsync(
        string userId,
        string permission,
        CancellationToken cancellation = default
    );

    public Task<Result> RevokeUserPermissionAsync(
        string userId,
        string permissionId,
        CancellationToken cancellation = default
    );

    public Task<Result> RemoveUserFromGroupAsync(
        string userId,
        string groupId,
        CancellationToken cancellation = default
    );
}