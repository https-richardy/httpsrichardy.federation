namespace HttpsRichardy.Federation.Application.Mappers;

public static class RealmMapper
{
    public static Realm AsRealm(RealmCreationScheme realm, string clientId, string secretHash) => new()
    {
        Name = realm.Name,
        Description = realm.Description ?? string.Empty,
        ClientId = clientId,
        SecretHash = secretHash
    };

    public static Realm AsRealm(RealmUpdateScheme payload, Realm realm)
    {
        realm.Name = payload.Name;
        realm.Description = payload.Description ?? realm.Description;

        realm.MarkAsUpdated();

        return realm;
    }

    public static RealmDetailsScheme AsResponse(Realm realm) => new()
    {
        Id = realm.Id.ToString(),
        Name = realm.Name,
        Description = realm.Description,
        ClientId = realm.ClientId,
        ClientSecret = realm.SecretHash
    };

    public static RealmFilters AsFilters(RealmFetchParameters parameters) => new()
    {
        Id = parameters.Id,
        ClientId = parameters.ClientId,
        Name = parameters.Name,
        Pagination = parameters.Pagination,
        Sort = parameters.Sort,
        IsDeleted = parameters.IsDeleted
    };
}
