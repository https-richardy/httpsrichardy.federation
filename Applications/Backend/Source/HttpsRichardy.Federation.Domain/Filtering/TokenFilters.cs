namespace HttpsRichardy.Federation.Domain.Filtering;

public sealed class TokenFilters : Filters
{
    public TokenType? Type { get; set; }
    public string? UserId { get; set; }
    public string? RealmId { get; set; }
    public string? Value { get; set; }

    public static TokenFiltersBuilder WithSpecifications() => new();
}