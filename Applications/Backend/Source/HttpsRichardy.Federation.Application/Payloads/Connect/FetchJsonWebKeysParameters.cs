namespace HttpsRichardy.Federation.Application.Payloads.Connect;

public sealed record FetchJsonWebKeysParameters : IDispatchable<Result<JsonWebKeySetScheme>>
{
    public string Realm { get; init; } = default!;
}
