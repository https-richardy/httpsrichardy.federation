namespace HttpsRichardy.Federation.Application.Payloads.Permission;

public sealed record PermissionUpdateScheme : IDispatchable<Result<PermissionDetailsScheme>>
{
    [JsonIgnore]
    public string PermissionId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}