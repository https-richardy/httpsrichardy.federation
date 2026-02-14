namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RevokeRealmPermissionScheme : IDispatchable<Result>
{
    [JsonIgnore]
    public string RealmId { get; init; } = default!;
    public string PermissionId { get; init; } = default!;
}
