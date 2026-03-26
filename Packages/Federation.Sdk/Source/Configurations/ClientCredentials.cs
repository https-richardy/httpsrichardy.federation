namespace HttpsRichardy.Federation.Sdk.Configurations;

public sealed record ClientCredentials
{
    public string ClientId { get; init; } = default!;
    public string ClientSecret { get; init; } = default!;
    public string Realm { get; init; } = default!;
}
