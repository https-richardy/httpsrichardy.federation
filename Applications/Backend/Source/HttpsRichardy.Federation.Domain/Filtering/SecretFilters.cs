namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class SecretFilters : Filters
{
    public string? RealmId { get; set; }
    public bool? CanSign { get; set; }
    public bool? InGracePeriod { get; set; }
    public bool? IsExpired { get; set; }
    public DateTime? Now { get; set; }

    public static SecretFilters WithoutFilters => new();
    public static SecretFiltersBuilder WithSpecifications() => new();
}
