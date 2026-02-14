namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record RevokeGroupPermissionScheme : IDispatchable<Result>
{
    public string PermissionId { get; init; } = default!;
    public string GroupId { get; init; } = default!;
}
