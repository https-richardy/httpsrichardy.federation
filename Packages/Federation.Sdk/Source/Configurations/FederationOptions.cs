namespace HttpsRichardy.Federation.Sdk.Configurations;

public sealed record FederationOptions
{
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string Realm { get; set; } = default!;
    public string Authority { get; set; } = default!;
    public string[] Audiences { get; set; } = [];
}
