namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class ClientFiltersBuilder : FiltersBuilderBase<ClientFilters, ClientFiltersBuilder>
{
    public ClientFiltersBuilder WithName(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name))
            _filters.Name = name.Trim().Normalize(NormalizationForm.FormC);

        return this;
    }
}