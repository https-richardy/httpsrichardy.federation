namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.User;

public sealed record ListUserAssignedPermissionsParameters
{
    public string? PermissionName { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 60;
}
