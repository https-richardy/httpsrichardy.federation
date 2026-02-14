namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record ClientAuthenticationCredentials : IDispatchable<Result<ClientAuthenticationResult>>
{
    public string GrantType { get; init; } = default!;

    // for client_credentials grant type
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;

    // for authorization_code grant type
    public string Code { get; init; } = default!;
    public string CodeVerifier { get; init; } = default!;
}
