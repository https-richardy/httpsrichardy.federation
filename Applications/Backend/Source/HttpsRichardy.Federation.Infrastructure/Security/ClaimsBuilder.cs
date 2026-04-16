namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class ClaimsBuilder
{
    private readonly List<Claim> _claims = [];
    public IEnumerable<Claim> Build() => _claims;

    public ClaimsBuilder WithSubject(string subject)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
        return this;
    }

    public ClaimsBuilder WithUsername(string username)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.PreferredUsername, username));
        return this;
    }

    public ClaimsBuilder WithClientId(string clientId)
    {
        _claims.Add(new Claim(IdentityClaimNames.ClientId, clientId));
        return this;
    }

    public ClaimsBuilder WithRealmName(string realmName)
    {
        _claims.Add(new Claim(IdentityClaimNames.Realm, realmName));
        return this;
    }

    public ClaimsBuilder WithPermissions(IEnumerable<Permission> permissions)
    {
        foreach (var permission in permissions)
        {
            _claims.Add(new Claim(ClaimTypes.Role, permission.Name));
        }

        return this;
    }

    public ClaimsBuilder WithClaim(string type, string value)
    {
        _claims.Add(new Claim(type, value));
        return this;
    }
}
