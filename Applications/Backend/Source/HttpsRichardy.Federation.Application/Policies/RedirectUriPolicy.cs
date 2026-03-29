namespace HttpsRichardy.Federation.Application.Policies;

public sealed class RedirectUriPolicy : IRedirectUriPolicy
{
    public Task<Result> EnsureRedirectUriIsAllowedAsync(
        Realm realm, RedirectUri redirectUri, CancellationToken cancellation = default)
    {
        // according to oauth 2.0 spec (RFC 6749, section 3.1.2.3):
        // https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2.3

        var isAllowed = realm.RedirectUris.Contains(redirectUri);

        return isAllowed ?
            Task.FromResult(Result.Success()) :
            Task.FromResult(Result.Failure(AuthorizationErrors.RedirectUriNotAllowed));
    }
}
