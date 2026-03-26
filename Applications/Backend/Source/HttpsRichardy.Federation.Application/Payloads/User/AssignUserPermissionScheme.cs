namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record AssignUserPermissionScheme : IDispatchable<Result>
{
    [JsonIgnore]
    public string UserId { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
}
