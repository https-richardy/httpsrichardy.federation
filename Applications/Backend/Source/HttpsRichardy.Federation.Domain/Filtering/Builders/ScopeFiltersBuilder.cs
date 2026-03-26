namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class ScopeFiltersBuilder : FiltersBuilderBase<ScopeFilters, ScopeFiltersBuilder>
{
    public ScopeFiltersBuilder WithName(string? name)
    {
        _filters.Name = name;

        return this;
    }

    public ScopeFiltersBuilder WithRealmId(string? realmId)
    {
        _filters.RealmId = realmId;

        return this;
    }

    public ScopeFiltersBuilder WithDescription(string? description)
    {
        _filters.Description = description;

        return this;
    }
}
