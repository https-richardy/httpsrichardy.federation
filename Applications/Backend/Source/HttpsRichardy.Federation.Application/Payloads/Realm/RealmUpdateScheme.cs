namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RealmUpdateScheme : IDispatchable<Result<RealmDetailsScheme>>
{
    [JsonIgnore]
    public string RealmId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
}
