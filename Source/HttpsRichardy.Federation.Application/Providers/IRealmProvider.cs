namespace HttpsRichardy.Federation.Application.Providers;

public interface IRealmProvider
{
    public string? Realm { get; }

    public void SetRealm(Realm realm);
    public Realm GetCurrentRealm();
}
