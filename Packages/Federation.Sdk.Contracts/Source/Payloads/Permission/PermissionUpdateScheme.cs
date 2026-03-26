namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Permission;

public sealed record PermissionUpdateScheme
{
    [JsonIgnore]
    public string PermissionId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}
