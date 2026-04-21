namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ClientScheme
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;

    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;

    public IEnumerable<Grant> Flows { get; set; } = [];
    public IEnumerable<RedirectUri> RedirectUris { get; set; } = [];
}
