namespace HttpsRichardy.Federation.Domain.Policies;

// according to oauth 2.0 spec (RFC 6749, section 3.1.2.3):
// this interface defines a policy to ensure that a given redirect URI is allowed for a specific realm

// https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2.3

public interface IRedirectUriPolicy
{
    public Task<Result> EnsureRedirectUriIsAllowedAsync(
        Realm realm,
        RedirectUri redirectUri,
        CancellationToken cancellation = default
    );
}
