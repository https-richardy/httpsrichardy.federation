namespace HttpsRichardy.Federation.Infrastructure.Providers;

public sealed class RealmProvider : IRealmProvider
{
    private Realm? _currentRealm;
    public string? Realm => _currentRealm?.Name;

    public void SetRealm(Realm realm) =>
        _currentRealm = realm;

    public Realm GetCurrentRealm() =>
        _currentRealm!;
}
