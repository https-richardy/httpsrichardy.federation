namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Group : Aggregate
{
    public string RealmId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public ICollection<Permission> Permissions { get; set; } = [];
}
