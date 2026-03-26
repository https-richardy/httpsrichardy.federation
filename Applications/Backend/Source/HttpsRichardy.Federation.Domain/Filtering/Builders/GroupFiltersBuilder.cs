namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class GroupFiltersBuilder :
    FiltersBuilderBase<GroupFilters, GroupFiltersBuilder>
{
    public GroupFiltersBuilder WithIdentifiers(string[] identifiers)
    {
        var validIdentifiers = identifiers?
            .Where(identifier => !string.IsNullOrWhiteSpace(identifier))
            .ToArray();

        if (validIdentifiers?.Length > 0)
            _filters.Identifiers = validIdentifiers;

        return this;
    }

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
