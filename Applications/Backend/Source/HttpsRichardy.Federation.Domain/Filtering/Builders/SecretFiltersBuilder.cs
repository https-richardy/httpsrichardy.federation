namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class SecretFiltersBuilder :
    FiltersBuilderBase<SecretFilters, SecretFiltersBuilder>
{
    public SecretFiltersBuilder WithRealm(string? realmId)
    {
        if (!string.IsNullOrWhiteSpace(realmId))
            _filters.RealmId = realmId;

        return this;
    }

    public SecretFiltersBuilder WithCanSign(DateTime? now = null)
    {
        _filters.CanSign = true;
        _filters.Now = now ?? DateTime.UtcNow;

        return this;
    }

    public SecretFiltersBuilder WithInGrace(DateTime? now = null)
    {
        _filters.InGracePeriod = true;
        _filters.Now = now ?? DateTime.UtcNow;

        return this;
    }

    public SecretFiltersBuilder WithExpired(DateTime? now = null)
    {
        _filters.IsExpired = true;
        _filters.Now = now ?? DateTime.UtcNow;

        return this;
    }
}
