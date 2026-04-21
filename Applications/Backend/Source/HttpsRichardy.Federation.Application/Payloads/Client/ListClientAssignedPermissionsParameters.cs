namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ListClientAssignedPermissionsParameters :
    IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public string? Id { get; init; }
}
