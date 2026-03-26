namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RealmDeletionScheme : IDispatchable<Result>
{
    public string RealmId { get; init; } = default!;
}
