namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record RevokeClientPermissionScheme : IDispatchable<Result>
{
    public string PermissionId { get; init; } = default!;
    public string Id { get; init; } = default!;
}
