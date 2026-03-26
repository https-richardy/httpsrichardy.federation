namespace HttpsRichardy.Federation.Application.Payloads.Permission;

public sealed record PermissionDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}