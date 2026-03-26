namespace HttpsRichardy.Federation.Application.Payloads.Permission;

public sealed record PermissionDeletionScheme : IDispatchable<Result>
{
    public string PermissionId { get; init; } = default!;
}
