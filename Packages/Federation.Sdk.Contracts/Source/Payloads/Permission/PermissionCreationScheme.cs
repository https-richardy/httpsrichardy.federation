namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Permission;

public sealed record PermissionCreationScheme
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}
