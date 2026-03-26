namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class TokenFiltersBuilder : FiltersBuilderBase<TokenFilters, TokenFiltersBuilder>
{
    public TokenFiltersBuilder WithValue(string? value)
    {
        _filters.Value = value;

        return this;
    }

    public TokenFiltersBuilder WithType(TokenType? type)
    {
        _filters.Type = type;

        return this;
    }

    public TokenFiltersBuilder WithUserId(string? userId)
    {
        _filters.UserId = userId;

        return this;
    }

    public TokenFiltersBuilder WithRealmId(string? realmId)
    {
        _filters.RealmId = realmId;

        return this;
    }
}
