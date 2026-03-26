namespace HttpsRichardy.Federation.Application.Providers;

public interface IPrincipalProvider
{
    public User? Principal { get; }

    public void SetPrincipal(User user);
    public void Clear();
    public User GetCurrentPrincipal();
}
