namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class GroupFilters : Filters
{
    public string? RealmId { get; set; }
    public string? Name { get; set; }

    public static GroupFiltersBuilder WithSpecifications() => new();
}
