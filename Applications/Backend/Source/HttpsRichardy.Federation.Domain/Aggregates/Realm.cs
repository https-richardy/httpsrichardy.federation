namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Realm : Aggregate
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public string ClientId { get; set; } = default!;
    public string SecretHash { get; set; } = default!;

    public ICollection<Permission> Permissions { get; set; } = [];
    public ICollection<RedirectUri> RedirectUris { get; set; } = [];
}
