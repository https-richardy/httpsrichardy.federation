namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record ListGroupAssignedPermissionsParameters :
    IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public string GroupId { get; init; } = default!;
    public string? PermissionName { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
