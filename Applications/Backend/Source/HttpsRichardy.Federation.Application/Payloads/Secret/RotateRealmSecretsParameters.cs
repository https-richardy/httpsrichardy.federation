namespace HttpsRichardy.Federation.Application.Payloads.Secret;

public sealed record RotateRealmSecretsParameters :
    IDispatchable<Result>
{
    public string RealmId { get; init; } = default!;
}
