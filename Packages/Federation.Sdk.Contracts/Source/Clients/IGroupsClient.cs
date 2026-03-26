namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IGroupsClient
{
    public Task<Result<Pagination<GroupDetails>>> GetGroupsAsync(
        GroupsFetchParameters? parameters = null,
        CancellationToken cancellation = default
    );

    public Task<Result<GroupDetails>> CreateGroupAsync(
        GroupCreationScheme group,
        CancellationToken cancellation = default
    );

    public Task<Result<GroupDetails>> UpdateGroupAsync(
        GroupUpdateScheme group,
        CancellationToken cancellation = default
    );

    public Task<Result> DeleteGroupAsync(
        string groupId,
        CancellationToken cancellation = default
    );

    public Task<Result<GroupDetails>> AssignGroupPermissionAsync(
        string groupId,
        string permission,
        CancellationToken cancellation = default
    );

    public Task<Result> RevokeGroupPermissionAsync(
        string groupId,
        string permissionId,
        CancellationToken cancellation = default
    );

    public Task<Result<IReadOnlyCollection<PermissionDetails>>> GetGroupPermissionsAsync(
        string groupId,
        ListGroupPermissionsParameters? parameters = null,
        CancellationToken cancellation = default
    );
}
