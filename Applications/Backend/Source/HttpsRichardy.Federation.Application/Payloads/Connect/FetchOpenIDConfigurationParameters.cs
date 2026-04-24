namespace HttpsRichardy.Federation.Application.Payloads.Connect;

public sealed record FetchOpenIDConfigurationParameters :
    IDispatchable<Result<OpenIDConfigurationScheme>>
{
    public string Realm { get; init; } = default!;
}
