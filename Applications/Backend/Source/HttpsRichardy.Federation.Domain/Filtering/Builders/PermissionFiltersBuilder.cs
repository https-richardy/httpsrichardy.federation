namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class PermissionFiltersBuilder :
    FiltersBuilderBase<PermissionFilters, PermissionFiltersBuilder>
{
    public PermissionFiltersBuilder WithName(string? name)
    {
        _filters.Name = name;

        return this;
    }

    public PermissionFiltersBuilder WithRealmId(string? realmId)
    {
        _filters.RealmId = realmId;

        return this;
    }
}
