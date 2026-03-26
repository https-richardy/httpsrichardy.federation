namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Realm;

public sealed record RealmUpdateScheme
{
    [JsonIgnore]
    public string RealmId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}