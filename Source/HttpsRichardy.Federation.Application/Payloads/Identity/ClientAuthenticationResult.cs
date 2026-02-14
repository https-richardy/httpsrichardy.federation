namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record ClientAuthenticationResult
{
    [property: JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;
}