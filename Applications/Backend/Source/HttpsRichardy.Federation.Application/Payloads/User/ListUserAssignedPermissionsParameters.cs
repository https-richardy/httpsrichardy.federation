namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record ListUserAssignedPermissionsParameters :
    IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public string UserId { get; init; } = default!;
    public string? PermissionName { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
