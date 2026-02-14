namespace HttpsRichardy.Federation.TestSuite.Integration.Security;

public sealed class AuthenticationServiceTests :
    IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly UserCollection _userCollection;
    private readonly PasswordHasher _passwordHasher;
    private readonly ISecurityTokenService _tokenService;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly AuthenticationService _authenticationService;
    private readonly Fixture _fixture = new();
    private readonly RSA _rsa = RSA.Create(2048);

    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Mock<IHostInformationProvider> _hostProvider = new();
    private readonly Mock<ISecretCollection> _secretCollection = new();
    private readonly Mock<IGroupCollection> _groupCollection = new();

    public AuthenticationServiceTests(MongoDatabaseFixture mongoFixture)
    {
        _mongoFixture = mongoFixture;
        _database = mongoFixture.Database;

        _userCollection = new UserCollection(_database, _realmProvider.Object);
        _passwordHasher = new PasswordHasher();

        var tokenCollection = new TokenCollection(_database, _realmProvider.Object);
        var secret = new Secret
        {
            PrivateKey = Convert.ToBase64String(_rsa.ExportRSAPrivateKey()),
            PublicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey())
        };

        var realm = _fixture.Create<Realm>();

        _secretCollection
            .Setup(collection => collection.GetSecretAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(secret);

        _groupCollection
            .Setup(collection => collection.GetGroupsAsync(It.IsAny<GroupFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _hostProvider.Setup(provider => provider.Address)
            .Returns(new Uri("http://localhost:5078"));

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        _tokenService = new JwtSecurityTokenService(
            secretCollection: _secretCollection.Object,
            tokenCollection: tokenCollection,
            realmProvider: _realmProvider.Object,
            groupCollection: _groupCollection.Object,
            host: _hostProvider.Object
        );

        _authenticationService = new AuthenticationService(
            userCollection: _userCollection,
            passwordHasher: _passwordHasher,
            tokenService: _tokenService
        );
    }

    [Fact(DisplayName = "[security] - when valid credentials are provided, then returns access and refresh tokens")]
    public async Task WhenValidCredentialsAreProvided_ThenReturnsAccessAndRefreshTokens()
    {
        /* arrange: create a user with a known password and insert into collection */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var plainPassword = "My$ecureP@ssw0rd";
        var user = _fixture.Build<User>()
            .With(user => user.PasswordHash, await _passwordHasher.HashPasswordAsync(plainPassword))
            .With(user => user.RealmId, realm.Id)
            .With(user => user.IsDeleted, false)
            .Create();

        await _userCollection.InsertAsync(user);

        /* act: authenticate with valid credentials */
        var credentials = new AuthenticationCredentials
        {
            Username = user.Username,
            Password = plainPassword
        };

        var result = await _authenticationService.AuthenticateAsync(credentials);

        /* assert: authentication should be successful and tokens should be returned */
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        Assert.NotNull(result.Data.AccessToken);
        Assert.NotNull(result.Data.RefreshToken);
    }

    [Fact(DisplayName = "[security] - when invalid password is provided, then returns error (#ERROR-A7E7C)")]
    public async Task AuthenticateAsync_InvalidPassword_ReturnsInvalidCredentialsError()
    {
        /* arrange: create a user with a known password */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var plainPassword = "My$ecureP@ssw0rd";
        var user = _fixture.Build<User>()
            .With(user => user.PasswordHash, await _passwordHasher.HashPasswordAsync(plainPassword))
            .With(user => user.IsDeleted, false)
            .With(user => user.RealmId, realm.Id)
            .Create();

        await _userCollection.InsertAsync(user);

        var credentials = new AuthenticationCredentials
        {
            Username = user.Username,
            Password = "wrong-password"
        };

        /* act: attempt to authenticate with wrong password */
        var result = await _authenticationService.AuthenticateAsync(credentials);

        /* assert: it should fail with invalid credentials error */
        Assert.True(result.IsFailure);
        Assert.Equal(AuthenticationErrors.InvalidCredentials, result.Error);
    }

    [Fact(DisplayName = "[security] - when user does not exist, then returns invalid credentials error (#ERROR-A7E7C)")]
    public async Task WhenUserDoesNotExist_ThenReturnsInvalidCredentialsError()
    {
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        /* arrange: create credentials with a username that does not exist */
        var credentials = new AuthenticationCredentials
        {
            Username = "nonexistentuser",
            Password = "any-password"
        };

        /* act: attempt to authenticate with non-existent user */
        var result = await _authenticationService.AuthenticateAsync(credentials);

        /* assert: it should fail with invalid credentials error */
        Assert.True(result.IsFailure);
        Assert.Equal(AuthenticationErrors.InvalidCredentials, result.Error);
    }

    public async Task InitializeAsync() => await _mongoFixture.CleanDatabaseAsync();
    public async Task DisposeAsync() => await Task.CompletedTask;
}
