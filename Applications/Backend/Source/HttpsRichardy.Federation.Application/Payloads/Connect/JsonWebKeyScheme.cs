namespace HttpsRichardy.Federation.Application.Payloads.Connect;

public sealed record JsonWebKeyScheme
{
    [JsonPropertyName("kty")]
    public string Type { get; set; } = "RSA";

    [JsonPropertyName("use")]
    public string Use { get; set; } = "sig";

    [JsonPropertyName("kid")]
    public string Identifier { get; set; } = default!;

    [JsonPropertyName("e")]
    public string Exponent { get; set; } = default!;

    [JsonPropertyName("n")]
    public string Modulus { get; set; } = default!;
}