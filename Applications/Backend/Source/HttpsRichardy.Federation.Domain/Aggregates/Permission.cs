namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Permission : Aggregate
{
    public string RealmId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; } = default!;
}
