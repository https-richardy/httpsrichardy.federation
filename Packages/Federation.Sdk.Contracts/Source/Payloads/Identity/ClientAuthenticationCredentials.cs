namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Identity;

public sealed record ClientAuthenticationCredentials
{
    public string GrantType { get; init; } = "client_credentials";
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
}