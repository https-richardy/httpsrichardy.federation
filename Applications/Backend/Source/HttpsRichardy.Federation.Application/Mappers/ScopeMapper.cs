namespace HttpsRichardy.Federation.Application.Mappers;

public static class ScopeMapper
{
    public static Scope AsScope(ScopeCreationScheme scope, Realm realm) => new()
    {
        Name = scope.Name,
        Description = scope.Description,
        RealmId = realm.Id,
        IsGlobal = false
    };

    public static ScopeDetailsScheme AsResponse(Scope scope) => new()
    {
        Id = scope.Id,
        Name = scope.Name,
        Description = scope.Description,
    };
}