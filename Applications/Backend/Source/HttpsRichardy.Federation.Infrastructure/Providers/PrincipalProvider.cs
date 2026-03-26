namespace HttpsRichardy.Federation.Infrastructure.Providers;

public sealed class PrincipalProvider : IPrincipalProvider
{
    private User? _currentPrincipal;
    public User? Principal => _currentPrincipal;

    public void SetPrincipal(User user) =>
        _currentPrincipal = user;

    public User GetCurrentPrincipal() =>
        _currentPrincipal!;

    public void Clear() =>
        _currentPrincipal = null;
}
