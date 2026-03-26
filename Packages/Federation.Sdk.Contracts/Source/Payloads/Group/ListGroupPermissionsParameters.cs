namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record ListGroupPermissionsParameters
{
    public string? PermissionName { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 60;
}
