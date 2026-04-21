namespace HttpsRichardy.Federation.TestSuite.Integration.Security;

public sealed class JwtSecurityTokenServiceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly JwtSecurityTokenService _jwtSecurityTokenService;
    private readonly TokenCollection _tokenCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly RSA _rsa = RSA.Create(2048);
    private readonly Fixture _fixture = new();

    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Mock<ISecretCollection> _secretCollection = new();
    private readonly Mock<IHostInformationProvider> _hostProvider = new();
    private readonly Mock<IGroupCollection> _groupCollection = new();

    public JwtSecurityTokenServiceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;

        _tokenCollection = new TokenCollection(_database, _realmProvider.Object);
        _hostProvider
            .Setup(provider => provider.Address)
            .Returns(new Uri("http://localhost:5078"));

        var realm = _fixture.Create<Realm>();
        var secret = new Secret
        {
            Id = Identifier.Generate<Secret>(),
            RealmId = realm.Id,
            PrivateKey = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()),
            PublicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey()),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        _groupCollection
            .Setup(collection => collection.GetGroupsAsync(It.IsAny<GroupFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _secretCollection
            .Setup(collection => collection.GetSecretsAsync(It.IsAny<SecretFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([secret]);

        _jwtSecurityTokenService = new JwtSecurityTokenService(
            realmProvider: _realmProvider.Object,
            secretCollection: _secretCollection.Object,
            groupCollection: _groupCollection.Object,
            tokenCollection: _tokenCollection,
            host: _hostProvider.Object
        );
    }

    [Fact(DisplayName = "[infrastructure] - when generating an access token, then it must be valid and contain correct claims")]
    public async Task WhenGeneratingAccessToken_ThenItMustBeValidAndContainCorrectClaims()
    {
        /* arrange: create a user */
        var user = _fixture.Create<User>();
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        /* act: generate an access token */
        var result = await _jwtSecurityTokenService.GenerateAccessTokenAsync(user);

        /* assert: token must be successful and valid */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var validationResult = await _jwtSecurityTokenService.ValidateTokenAsync(result.Data);

        Assert.True(validationResult.IsSuccess);

        /* assert: token claims must be correct */
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(result.Data.Value);

        var claims = jwtToken.Claims.ToList();

        Assert.Contains(claims, claim => claim.Type == JwtRegisteredClaimNames.Aud && claim.Value == realm.Name);
        Assert.Contains(claims, claim => claim.Type == JwtRegisteredClaimNames.Iss && claim.Value == _hostProvider.Object.Address.ToString().TrimEnd('/'));
        Assert.Contains(claims, claim => claim.Type == JwtRegisteredClaimNames.Sub && claim.Value == user.Id.ToString());
        Assert.Contains(claims, claim => claim.Type == JwtRegisteredClaimNames.PreferredUsername && claim.Value == user.Username);

        /* assert that a claim containing the realm name is present */
        Assert.Contains(claims, claim => claim.Type == IdentityClaimNames.Realm && claim.Value == realm.Name);

        foreach (var permission in user.Permissions)
        {
            Assert.Contains(claims, claim => claim.Type == "role" && claim.Value == permission.Name);
        }
    }

    [Fact(DisplayName = "[infrastructure] - when generating a refresh token, then it must be valid and contain correct claims and be persisted")]
    public async Task WhenGeneratingRefreshToken_ThenItMustBeValidAndContainCorrectClaimsAndBePersisted()
    {
        /* arrange: create a user */
        var realm = _fixture.Create<Realm>();
        var user = _fixture.Build<User>()
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        /* act: generate a refresh token */
        var result = await _jwtSecurityTokenService.GenerateRefreshTokenAsync(user);

        /* assert: token must be successful and valid */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var validationResult = await _jwtSecurityTokenService.ValidateTokenAsync(result.Data);

        Assert.True(validationResult.IsSuccess);

        /* assert: refresh token must be persisted in the database */
        var filters = TokenFilters.WithSpecifications()
            .WithValue(result.Data.Value)
            .Build();

        var tokens = await _tokenCollection.GetTokensAsync(filters);

        Assert.Single(tokens);
        Assert.Equal(result.Data.Value, tokens.First().Value);
    }

    [Fact(DisplayName = "[infrastructure] - when validating an expired token, then it must return token expired error")]
    public async Task WhenValidatingExpiredToken_ThenItMustReturnTokenExpiredError()
    {
        /* arrange: create an expired token */

        var secret = new Secret
        {
            Id = Identifier.Generate<Secret>(),
            PrivateKey = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()),
            PublicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey())
        };

        var privateKey = Common.Utilities.RsaHelper.CreateSecurityKeyFromPrivateKey(secret.PrivateKey);

        var tokenHandler = new JwtSecurityTokenHandler();
        var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, Identifier.Generate<User>())]),
            NotBefore = DateTime.UtcNow.AddMinutes(-2),
            Expires = DateTime.UtcNow.AddMinutes(-1),
            SigningCredentials = credentials
        };

        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var expiredToken = new Domain.Aggregates.SecurityToken
        {
            Value = tokenHandler.WriteToken(token),
            ExpiresAt = tokenDescriptor.Expires.Value
        };

        /* act: validate the expired token */
        var result = await _jwtSecurityTokenService.ValidateTokenAsync(expiredToken);

        /* assert: result must be a failure with TokenExpired error */
        Assert.True(result.IsFailure);
        Assert.Equal(AuthenticationErrors.TokenExpired, result.Error);
    }

    [Fact(DisplayName = "[infrastructure] - when validating a token with invalid format, then it must return invalid token format error")]
    public async Task WhenValidatingTokenWithInvalidFormat_ThenItMustReturnInvalidTokenFormatError()
    {
        /* arrange: create a token with invalid format */
        var invalidFormatToken = new Domain.Aggregates.SecurityToken
        {
            Value = "invalid-token-format",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        /* act: validate the token with invalid format */
        var result = await _jwtSecurityTokenService.ValidateTokenAsync(invalidFormatToken);

        /* assert: result must be a failure with InvalidTokenFormat error */
        Assert.True(result.IsFailure);
        Assert.Equal(AuthenticationErrors.InvalidTokenFormat, result.Error);
    }

    [Fact(DisplayName = "[infrastructure] - when revoking a refresh token, then it must be marked as revoked and deleted in the database")]
    public async Task WhenRevokingRefreshToken_ThenItMustBeMarkedAsRevokedAndDeletedInTheDatabase()
    {
        /* arrange: create and insert a refresh token */
        var realm = _fixture.Create<Realm>();
        var user = _fixture.Build<User>()
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var refreshTokenResult = await _jwtSecurityTokenService.GenerateRefreshTokenAsync(user);

        Assert.True(refreshTokenResult.IsSuccess);
        Assert.NotNull(refreshTokenResult.Data);

        var refreshToken = refreshTokenResult.Data;

        /* act: revoke the refresh token */
        var revokeResult = await _jwtSecurityTokenService.RevokeRefreshTokenAsync(refreshToken);

        /* assert: revoke must be successful */
        Assert.True(revokeResult.IsSuccess);

        /* assert: refresh token must be marked as revoked and deleted in the database */
        var filters = TokenFilters.WithSpecifications()
            .WithValue(refreshToken.Value)
            .WithIsDeleted(true)
            .Build();

        var tokens = await _tokenCollection.GetTokensAsync(filters);

        Assert.Single(tokens);

        Assert.True(tokens.First().Revoked);
        Assert.True(tokens.First().IsDeleted);
    }

    [Fact(DisplayName = "[infrastructure] - when revoking a non-existent refresh token, then it must return invalid refresh token error")]
    public async Task WhenRevokingNonExistentRefreshToken_ThenItMustReturnInvalidRefreshTokenError()
    {
        /* arrange: create a non-existent refresh token */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var nonExistentToken = _fixture.Create<Domain.Aggregates.SecurityToken>();

        /* act: revoke the non-existent refresh token */
        var revokeResult = await _jwtSecurityTokenService.RevokeRefreshTokenAsync(nonExistentToken);

        /* assert: revoke must be a failure with InvalidRefreshToken error */
        Assert.True(revokeResult.IsFailure);
        Assert.Equal(AuthenticationErrors.InvalidRefreshToken, revokeResult.Error);
    }

    [Fact(DisplayName = "[infrastructure] - ValidateAccessTokenAsync should call ValidateTokenAsync")]
    public async Task ValidateAccessTokenAsync_ShouldCallValidateTokenAsync()
    {
        /* arrange: create a valid token */
        var user = _fixture.Create<User>();
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var accessTokenResult = await _jwtSecurityTokenService.GenerateAccessTokenAsync(user);

        Assert.True(accessTokenResult.IsSuccess);
        Assert.NotNull(accessTokenResult.Data);

        var accessToken = accessTokenResult.Data;

        /* act: validate access token */
        var result = await _jwtSecurityTokenService.ValidateAccessTokenAsync(accessToken);

        /* assert: result must be successful */
        Assert.True(result.IsSuccess);
    }

    [Fact(DisplayName = "[infrastructure] - ValidateRefreshTokenAsync should call ValidateTokenAsync")]
    public async Task ValidateRefreshTokenAsync_ShouldCallValidateTokenAsync()
    {
        /* arrange: create a valid token */
        var user = _fixture.Create<User>();
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var refreshTokenResult = await _jwtSecurityTokenService.GenerateRefreshTokenAsync(user);
        var refreshToken = refreshTokenResult.Data;

        /* act: validate refresh token */
        var result = await _jwtSecurityTokenService.ValidateRefreshTokenAsync(refreshToken!);

        /* assert: result must be successful */
        Assert.True(result.IsSuccess);
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
