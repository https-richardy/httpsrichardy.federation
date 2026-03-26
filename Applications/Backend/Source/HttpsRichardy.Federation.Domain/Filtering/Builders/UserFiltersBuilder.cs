namespace HttpsRichardy.Federation.Domain.Filtering.Builders;

public sealed class UserFiltersBuilder : FiltersBuilderBase<UserFilters, UserFiltersBuilder>
{
    public UserFiltersBuilder WithRealmId(string? realmId)
    {
        _filters.RealmId = realmId;

        return this;
    }

    public UserFiltersBuilder WithSecurityToken(string? token)
    {
        _filters.SecurityToken = token;

        return this;
    }

    public UserFiltersBuilder WithUsername(string? username)
    {
        _filters.Username = username;
        return this;
    }
}
