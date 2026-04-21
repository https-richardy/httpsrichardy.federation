namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RealmDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
}
