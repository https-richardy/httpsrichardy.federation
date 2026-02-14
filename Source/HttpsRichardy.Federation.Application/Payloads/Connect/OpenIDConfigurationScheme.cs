namespace HttpsRichardy.Federation.Application.Payloads.Connect;

// https://openid.net/specs/openid-connect-discovery-1_0.html
// using JsonPropertyName attributes to serialize properties in snake_case as required by
// RFC 8414 (OpenID Connect Discovery)

public sealed record OpenIDConfigurationScheme
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = default!;

    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = default!;

    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; } = default!;

    [JsonPropertyName("userinfo_endpoint")]
    public string UserInfoEndpoint { get; set; } = default!;

    [JsonPropertyName("jwks_uri")]
    public string JwksUri { get; set; } = default!;

    [JsonPropertyName("response_types_supported")]
    public IEnumerable<string> ResponseTypesSupported { get; set; } = ["code", "token"];

    [JsonPropertyName("subject_types_supported")]
    public IEnumerable<string> SubjectTypesSupported { get; set; } = ["public"];

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public IEnumerable<string> IdTokenSigningAlgValuesSupported { get; set; } = ["RS256"];
}
