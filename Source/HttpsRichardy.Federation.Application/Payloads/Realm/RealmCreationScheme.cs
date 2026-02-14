namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RealmCreationScheme : IDispatchable<Result<RealmDetailsScheme>>
{
    public string Name { get; init; } = default!;
    public string? Description { get; init; } = default!;
}
