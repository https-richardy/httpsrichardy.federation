namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class ScopeFilters : Filters
{
    public string? RealmId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public static ScopeFiltersBuilder WithSpecifications() => new();
}
