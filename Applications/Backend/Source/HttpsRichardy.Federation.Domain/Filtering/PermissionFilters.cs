namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class PermissionFilters : Filters
{
    public string? RealmId { get; set; }
    public string? Name { get; set; }

    public static PermissionFiltersBuilder WithSpecifications() => new();
}