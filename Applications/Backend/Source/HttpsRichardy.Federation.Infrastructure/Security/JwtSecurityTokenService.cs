namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class JwtSecurityTokenService(
    ISecretCollection secretCollection,
    ITokenCollection tokenCollection,
    IRealmProvider realmProvider,
    IGroupCollection groupCollection,
    ISecretRotationService secretRotationService,
    IHostInformationProvider host
) : ISecurityTokenService
{
    private readonly TimeSpan _accessTokenDuration = TimeSpan.FromHours(2);
    private readonly TimeSpan _refreshTokenDuration = TimeSpan.FromDays(7);

    public async Task<Result<SecurityToken>> GenerateAccessTokenAsync(User user, IEnumerable<Audience> audiences, CancellationToken cancellation = default)
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
        var resolvedAudiences = audiences
            .Where(audience => !string.IsNullOrWhiteSpace(audience.Value))
            .Select(audience => audience.Value.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var claims = new ClaimsBuilder()
            .WithSubject(user.Id.ToString())
            .WithUsername(user.Username)
            .WithPermissions(permissions);

        var realm = realmProvider.GetCurrentRealm();
        var privateKey = await GetPrivateKeyAsync(cancellation);
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        claims.WithClaim(IdentityClaimNames.Realm, realm.Name);
        claims.WithClaim(IdentityClaimNames.RealmId, realm.Id);

        if (resolvedAudiences.Count > 0)
        {
            claims.WithAudiences(resolvedAudiences);
        }

        var claimsIdentity = new ClaimsIdentity(claims.Build());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = resolvedAudiences.Count > 0 ? null : realm.Name,
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

    public async Task<Result<SecurityToken>> GenerateAccessTokenAsync(User user, CancellationToken cancellation = default)
        => await GenerateAccessTokenAsync(user, [], cancellation);

    public async Task<Result<SecurityToken>> GenerateAccessTokenAsync(Client client, CancellationToken cancellation = default)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var realm = realmProvider.GetCurrentRealm();

        var claims = new ClaimsBuilder()
            .WithSubject(client.Id)
            .WithRealmName(realm.Name)
            .WithClientId(client.Id)
            .WithPermissions(client.Permissions)
            .WithAudiences(client.Audiences.Select(audience => audience.Value))
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
        var publicKeys = await GetPublicKeyAsync();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKeys = publicKeys,
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
        var realm = realmProvider.GetCurrentRealm();
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .WithCanSign()
            .Build();

        var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);
        var secret = secrets
            .OrderByDescending(secret => secret.CreatedAt)
            .FirstOrDefault();

        if (secret is null)
        {
            await secretRotationService.EnsureSecretExistsAsync(realm, cancellation);

            secrets = await secretCollection.GetSecretsAsync(filters, cancellation);
            secret = secrets
                .OrderByDescending(secret => secret.CreatedAt)
                .FirstOrDefault() ?? throw new InvalidOperationException($"no signing key available for realm '{realm.Id}'.");
        }

        var key = Common.Utilities.RsaHelper.CreateSecurityKeyFromPrivateKey(secret.PrivateKey);

        key.KeyId = secret.Id;

        return key;
    }

    private async Task<IReadOnlyCollection<RsaSecurityKey>> GetPublicKeyAsync(CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var filters = SecretFilters.WithSpecifications()
            .WithRealm(realm.Id)
            .WithCanValidate()
            .Build();

        var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);

        return [.. secrets.Select(secret =>
        {
            var key = Common.Utilities.RsaHelper.CreateSecurityKeyFromPublicKey(secret.PublicKey);

            key.KeyId = secret.Id;

            return key;
        })];
    }
}
