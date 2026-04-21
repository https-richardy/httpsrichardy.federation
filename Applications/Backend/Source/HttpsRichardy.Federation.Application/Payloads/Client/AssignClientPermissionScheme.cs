namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record AssignClientPermissionScheme :
    IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    [JsonIgnore]
    public string Id { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
}
