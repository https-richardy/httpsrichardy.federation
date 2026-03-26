namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record ListRealmAssignedPermissionsParameters :
    IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public string? RealmId { get; init; } = default!;
    public string? PermissionName { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
