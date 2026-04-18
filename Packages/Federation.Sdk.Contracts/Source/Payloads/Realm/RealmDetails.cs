namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Realm;

public sealed record RealmDetails
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}