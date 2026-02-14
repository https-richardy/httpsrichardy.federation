namespace HttpsRichardy.Federation.Application.Payloads.Permission;

public sealed record PermissionCreationScheme : IDispatchable<Result<PermissionDetailsScheme>>
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}