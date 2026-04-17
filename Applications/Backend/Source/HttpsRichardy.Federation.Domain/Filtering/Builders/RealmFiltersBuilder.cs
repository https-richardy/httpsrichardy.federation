namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class RealmFiltersBuilder :
    FiltersBuilderBase<RealmFilters, RealmFiltersBuilder>
{
    public RealmFiltersBuilder WithName(string? name)
    {
        _filters.Name = name;

        return this;
    }
}
