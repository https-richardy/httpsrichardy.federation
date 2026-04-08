namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Client : Aggregate
{
    public string Name { get; set; } = default!;
    public string Secret { get; set; } = default!;

    public ICollection<Grant> Flows { get; set; } = [];
    public ICollection<RedirectUri> RedirectUris { get; set; } = [];
    public ICollection<Permission> Permissions { get; set; } = [];

    public bool SupportsFlow(Grant flow) => Flows.Contains(flow);
    public bool HasRedirectUri(RedirectUri uri) => RedirectUris.Contains(uri);
}