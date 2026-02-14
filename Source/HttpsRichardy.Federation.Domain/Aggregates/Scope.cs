namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Scope : Aggregate
{
    public string RealmId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsGlobal { get; set; }
}