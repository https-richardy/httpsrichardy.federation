namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Realm : Aggregate
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;

    public ICollection<Client> Clients { get; set; } = [];
    public ICollection<Permission> Permissions { get; set; } = [];
}
