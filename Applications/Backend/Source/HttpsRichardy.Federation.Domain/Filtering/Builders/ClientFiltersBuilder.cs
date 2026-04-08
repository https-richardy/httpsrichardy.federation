namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class ClientFiltersBuilder : FiltersBuilderBase<ClientFilters, ClientFiltersBuilder>
{
    public ClientFiltersBuilder WithName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _filters.Name = name.Trim().Normalize(NormalizationForm.FormC);

        return this;
    }

    public ClientFiltersBuilder WithRealmId(string? realmId)
    {
        if (!string.IsNullOrWhiteSpace(realmId))
            _filters.RealmId = realmId.Trim().Normalize(NormalizationForm.FormC);

        return this;
    }
}