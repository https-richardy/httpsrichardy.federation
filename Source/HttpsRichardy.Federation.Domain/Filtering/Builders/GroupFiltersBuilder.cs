namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class GroupFiltersBuilder :
    FiltersBuilderBase<GroupFilters, GroupFiltersBuilder>
{
    public GroupFiltersBuilder WithRealmId(string? realmId)
    {
        _filters.RealmId = realmId;

        return this;
    }

    public GroupFiltersBuilder WithName(string? name)
    {
        _filters.Name = name;

        return this;
    }
}
