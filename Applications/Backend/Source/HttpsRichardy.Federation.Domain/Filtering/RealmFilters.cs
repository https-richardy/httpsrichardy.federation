namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class RealmFilters : Filters
{
    public string? Name { get; set; }

    public static RealmFiltersBuilder WithSpecifications() => new();
}
