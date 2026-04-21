namespace HttpsRichardy.Federation.Application.Payloads.Secret;

public sealed record FetchRealmSecretsParameters :
    IDispatchable<Result<IReadOnlyCollection<SecretScheme>>>
{
    public string RealmId { get; init; } = default!;
}
