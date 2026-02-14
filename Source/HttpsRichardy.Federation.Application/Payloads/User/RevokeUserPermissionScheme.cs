namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record RevokeUserPermissionScheme : IDispatchable<Result>
{
    public string UserId { get; init; } = default!;
    public string PermissionId { get; init; } = default!;
}
