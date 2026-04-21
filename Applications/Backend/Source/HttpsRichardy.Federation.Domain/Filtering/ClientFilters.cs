namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class ClientFilters : Filters
{
    public string? Name { get; set; }
    public string? ClientId { get; set; }
    public string? RealmId { get; set; }

    public static ClientFilters WithoutFilters => new();
    public static ClientFiltersBuilder WithSpecifications() => new();
}
