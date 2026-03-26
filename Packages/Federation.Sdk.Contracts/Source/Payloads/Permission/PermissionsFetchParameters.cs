namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Permission;

public sealed record PermissionsFetchParameters
{
    public string? Name { get; set; }
    public bool? IncludeDeleted { get; set; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
