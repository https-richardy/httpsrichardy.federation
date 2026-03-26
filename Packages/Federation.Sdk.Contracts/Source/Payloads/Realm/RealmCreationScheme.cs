namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Realm;

public sealed record RealmCreationScheme
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}
