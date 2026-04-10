namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class JwtSecurityTokenService(
    ISecretCollection secretCollection,
    ITokenCollection tokenCollection,
    IRealmProvider realmProvider,
    IGroupCollection groupCollection,
    IHostInformationProvider host
) : ISecurityTokenService
{
    private readonly TimeSpan _accessTokenDuration = TimeSpan.FromHours(2);
    private readonly TimeSpan _refreshTokenDuration = TimeSpan.FromDays(7);

    public async Task<Result<SecurityToken>> GenerateAccessTokenAsync(User user, CancellationToken cancellation = default)
    {
        var filters = GroupFilters.WithSpecifications()
            .WithRealmId(user.RealmId)
            .Build();

        var matchingGroups = await groupCollection.GetGroupsAsync(filters, cancellation);
        var groups = matchingGroups
            .Where(group => user.Groups.Any(userGroup => userGroup.Id == group.Id))
            .ToList();

        var groupPermissions = groups.SelectMany(group => group.Permissions ?? []);
        var permissions = user.Permissions
            .Concat(groupPermissions)
            .GroupBy(permission => permission.Name)
            .Select(group => group.First())
            .ToList();

        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new ClaimsBuilder()
            .WithSubject(user.Id.ToString())
            .WithUsername(user.Username)
            .WithPermissions(permissions);

        var realm = realmProvider.GetCurrentRealm();
        var privateKey = await GetPrivateKeyAsync(cancellation);
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        claims.WithClaim(IdentityClaimNames.Realm, realm.Name);
        claims.WithClaim(IdentityClaimNames.RealmId, realm.Id);

        var claimsIdentity = new ClaimsIdentity(claims.Build());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = realm.Name,
            Subject = claimsIdentity,
            Issuer = host.Address.ToString().TrimEnd('/'),
            SigningCredentials = credentials,
            NotBefore = DateTime.UtcNow.AddSeconds(-30),
            Expires = DateTime.UtcNow.Add(_accessTokenDuration),
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var securityToken = new SecurityToken
        {
            Value = tokenString,
            ExpiresAt = tokenDescriptor.Expires.Value
        };

        return Result<SecurityToken>.Success(securityToken);
    }

    public async Task<Result<SecurityToken>> GenerateAccessTokenAsync(Client client, CancellationToken cancellation = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new ClaimsBuilder()
            .WithRealmId(client.RealmId)
            .WithRealmName(client.Name)
            .WithPermissions(client.Permissions)
            .Build();

        var privateKey = await GetPrivateKeyAsync(cancellation);
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = host.Address.ToString().TrimEnd('/'),
            Audience = client.Name,
            Subject = claimsIdentity,
            SigningCredentials = credentials,
            NotBefore = DateTime.UtcNow.AddSeconds(-30),
            Expires = DateTime.UtcNow.Add(_accessTokenDuration)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var securityToken = new SecurityToken
        {
            Value = tokenString,
            ExpiresAt = tokenDescriptor.Expires.Value,
        };

        return Result<SecurityToken>.Success(securityToken);
    }

    public async Task<Result<SecurityToken>> GenerateRefreshTokenAsync(User user, CancellationToken cancellation = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var claims = new ClaimsBuilder()
            .WithSubject(user.Id.ToString())
            .WithUsername(user.Username)
            .WithPermissions(user.Permissions)
            .Build();

        var privateKey = await GetPrivateKeyAsync(cancellation);
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var realm = realmProvider.GetCurrentRealm();
        var claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = realm.Name,
            Subject = claimsIdentity,
            Issuer = host.Address.ToString().TrimEnd('/'),
            SigningCredentials = credentials,
            NotBefore = DateTime.UtcNow.AddSeconds(-30),
            Expires = DateTime.UtcNow.Add(_refreshTokenDuration)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        var securityToken = new SecurityToken
        {
            Type = TokenType.Refresh,
            UserId = user.Id,
            RealmId = user.RealmId,
            Value = tokenString,
            ExpiresAt = tokenDescriptor.Expires.Value,
        };

        await tokenCollection.InsertAsync(securityToken, cancellation: cancellation);

        return Result<SecurityToken>.Success(securityToken);
    }

    public async Task<Result> ValidateTokenAsync(SecurityToken token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var publicKey = await GetPublicKeyAsync();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = publicKey,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            tokenHandler.ValidateToken(token.Value, validationParameters, out _);
            return Result.Success();
        }
        catch (SecurityTokenExpiredException)
        {
            return Result.Failure(AuthenticationErrors.TokenExpired);
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return Result.Failure(AuthenticationErrors.InvalidSignature);
        }
        catch (ArgumentException)
        {
            return Result.Failure(AuthenticationErrors.InvalidTokenFormat);
        }
    }

    public async Task<Result> RevokeRefreshTokenAsync(SecurityToken token, CancellationToken cancellation = default)
    {
        var filters = TokenFilters.WithSpecifications()
            .WithValue(token.Value)
            .WithType(TokenType.Refresh)
            .Build();

        var tokens = await tokenCollection.GetTokensAsync(filters, cancellation);
        var existingToken = tokens.FirstOrDefault();

        if (existingToken is null)
        {
            return Result.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        if (existingToken.Revoked)
        {
            return Result.Failure(AuthenticationErrors.LogoutFailed);
        }

        existingToken.Revoked = true;
        existingToken.IsDeleted = true;

        await tokenCollection.UpdateAsync(existingToken, cancellation);

        return Result.Success();
    }

    public Task<Result> ValidateAccessTokenAsync(SecurityToken token, CancellationToken cancellation = default)
        => ValidateTokenAsync(token);
    public Task<Result> ValidateRefreshTokenAsync(SecurityToken token, CancellationToken cancellation = default)
        => ValidateTokenAsync(token);

    private async Task<RsaSecurityKey> GetPrivateKeyAsync(CancellationToken cancellation = default)
    {
        var secret = await secretCollection.GetSecretAsync(cancellation);
        return Common.Utilities.RsaHelper.CreateSecurityKeyFromPrivateKey(secret.PrivateKey);
    }

    private async Task<RsaSecurityKey> GetPublicKeyAsync(CancellationToken cancellation = default)
    {
        var secret = await secretCollection.GetSecretAsync(cancellation);
        return Common.Utilities.RsaHelper.CreateSecurityKeyFromPublicKey(secret.PublicKey);
    }
}
