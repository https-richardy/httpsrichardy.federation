namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record AssignRealmPermissionScheme : IDispatchable<Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    [JsonIgnore]
    public string RealmId { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
}
