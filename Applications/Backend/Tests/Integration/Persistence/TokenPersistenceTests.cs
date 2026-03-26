namespace HttpsRichardy.Federation.TestSuite.Integration.Persistence;

public sealed class TokenPersistenceTests : IClassFixture<MongoDatabaseFixture>, IAsyncLifetime
{
    private readonly ITokenCollection _tokenCollection;
    private readonly IMongoDatabase _database;
    private readonly MongoDatabaseFixture _mongoFixture;
    private readonly Mock<IRealmProvider> _realmProvider = new();
    private readonly Fixture _fixture = new();

    public TokenPersistenceTests(MongoDatabaseFixture fixture)
    {
        _mongoFixture = fixture;
        _database = fixture.Database;
        _tokenCollection = new TokenCollection(_database, _realmProvider.Object);
    }

    [Fact(DisplayName = "[infrastructure] - when inserting a token, then it must persist in the database")]
    public async Task WhenInsertingAToken_ThenItMustPersistInTheDatabase()
    {
        /* arrange: create token and matching filter */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "some-secret-token")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        var filters = TokenFilters.WithSpecifications()
            .WithValue(token.Value)
            .Build();

        /* act: persist token and query using value filter */
        await _tokenCollection.InsertAsync(token);

        var result = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);
        var retrievedToken = result.FirstOrDefault();

        /* assert: token must be retrieved with same id and value */
        Assert.NotNull(retrievedToken);
        Assert.Equal(token.Id, retrievedToken.Id);
        Assert.Equal(token.Value, retrievedToken.Value);
    }

    [Fact(DisplayName = "[infrastructure] - when updating a token, then updated fields must persist")]
    public async Task WhenUpdatingAToken_ThenUpdatedFieldsMustPersist()
    {
        /* arrange: create and insert token */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "update.test-token")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        await _tokenCollection.InsertAsync(token);

        /* act: update value and save */
        var newValue = "updated-token-value";

        token.Value = newValue;

        await _tokenCollection.UpdateAsync(token);

        var filters = TokenFilters.WithSpecifications()
            .WithValue(newValue)
            .Build();

        var result = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);
        var updatedToken = result.FirstOrDefault();

        /* assert: updated token must be found with new value */
        Assert.NotNull(updatedToken);

        Assert.Equal(token.Id, updatedToken.Id);
        Assert.Equal(newValue, updatedToken.Value);
    }

    [Fact(DisplayName = "[infrastructure] - when deleting a token, then it must be marked as deleted and not returned by filters")]
    public async Task WhenDeletingAToken_ThenItMustBeMarkedDeletedAndExcludedFromResults()
    {
        /* arrange: create and insert token */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "delete.test-token")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        await _tokenCollection.InsertAsync(token);

        var filters = TokenFilters.WithSpecifications()
            .WithValue(token.Value)
            .Build();

        /* act: delete token and query by value */
        var deleted = await _tokenCollection.DeleteAsync(token);

        var resultAfterDelete = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);

        /* assert: no tokens should be returned after delete */
        Assert.DoesNotContain(resultAfterDelete, token => token.Id == token.Id);

        /* arrange: prepare filters including deleted tokens */
        var filtersWithDeleted = TokenFilters.WithSpecifications()
            .WithValue(token.Value)
            .WithIsDeleted(true)
            .Build();

        /* act: refetch tokens including deleted */
        var resultWithDeleted = await _tokenCollection.GetTokensAsync(filtersWithDeleted, CancellationToken.None);

        /* assert: token should be returned when including deleted tokens */
        Assert.Contains(resultWithDeleted, token => token.Id == token.Id);

        Assert.True(token.IsDeleted);
        Assert.True(deleted);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering tokens, then it must return matching tokens")]
    public async Task WhenFilteringTokens_ThenItMustReturnOnlyMatchingTokens()
    {
        /* arrange: insert two tokens with different values */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token1 = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "filter1-token")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        var token2 = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "filter2-token")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        await _tokenCollection.InsertAsync(token1);
        await _tokenCollection.InsertAsync(token2);

        var filters = TokenFilters.WithSpecifications()
            .WithValue("filter1-token")
            .Build();

        /* act: query tokens filtered by value */
        var filteredTokens = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);

        /* assert: only token1 should be returned */
        Assert.Single(filteredTokens);
        Assert.Equal(token1.Id, filteredTokens.First().Id);
    }

    [Fact(DisplayName = "[infrastructure] - when paginating 10 tokens with page size 5, then it must return 5 tokens per page")]
    public async Task WhenPaginatingTenTokens_ThenItMustReturnFiveTokensPerPage()
    {
        /* arrange: create and insert 10 tokens, all not deleted */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var tokens = Enumerable.Range(1, 10)
            .Select(index => _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, $"token-{index}")
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create())
            .ToList();

        foreach (var token in tokens)
        {
            await _tokenCollection.InsertAsync(token);
        }

        /* arrange: prepare filters for page 1 with page size 5 */
        var filtersPage1 = TokenFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 1, pageSize: 5))
            .Build();

        /* act: get first page */
        var page1Results = await _tokenCollection.GetTokensAsync(filtersPage1, CancellationToken.None);

        /* assert: page 1 should return exactly 5 tokens */
        Assert.Equal(5, page1Results.Count);

        /* arrange: prepare filters for page 2 with page size 5 */
        var filtersPage2 = TokenFilters.WithSpecifications()
            .WithPagination(PaginationFilters.From(pageNumber: 2, pageSize: 5))
            .Build();

        /* act: get second page */
        var page2Results = await _tokenCollection.GetTokensAsync(filtersPage2, CancellationToken.None);

        /* assert: page 2 should return exactly 5 tokens */
        Assert.Equal(5, page2Results.Count);
    }

    [Fact(DisplayName = "[infrastructure] - when filtering tokens by realm id, then only tokens from that realm are returned")]
    public async Task WhenFilteringTokensByRealmId_ThenOnlyTokensFromThatRealmAreReturned()
    {
        /* arrange: create realm and token with realm id */
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "some-realm-token")
            .With(token => token.RealmId, realm.Id)
            .With(token => token.IsDeleted, false)
            .Create();

        await _tokenCollection.InsertAsync(token);

        var filters = TokenFilters.WithSpecifications()
            .WithRealmId(realm.Id)
            .WithValue(token.Value)
            .Build();

        /* act: query tokens filtered by realm id and value */
        var result = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);
        var retrievedToken = result.FirstOrDefault();

        /* assert: only token with matching realm id is returned */
        Assert.NotNull(retrievedToken);

        Assert.Equal(token.Id, retrievedToken.Id);
        Assert.Equal(token.Value, retrievedToken.Value);
        Assert.Equal(realm.Id, retrievedToken.RealmId);
    }

    [Theory(DisplayName = "[infrastructure] - when filtering tokens by type, then it must return matching tokens")]
    [InlineData(TokenType.Refresh)]
    [InlineData(TokenType.EmailVerification)]
    [InlineData(TokenType.PasswordReset)]
    public async Task WhenFilteringTokensByType_ThenItMustReturnOnlyMatchingTokens(TokenType type)
    {
        // arrange: insert one token with the requested type, and one with a different type
        var realm = _fixture.Create<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var matchingToken = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Type, type)
            .With(token => token.IsDeleted, false)
            .With(token => token.RealmId, realm.Id)
            .Create();

        var nonMatchingToken = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Type, type switch
            {
                TokenType.Refresh => TokenType.EmailVerification,
                TokenType.EmailVerification => TokenType.PasswordReset,
                TokenType.PasswordReset => TokenType.Refresh,

                _ => TokenType.Refresh
            })
            .With(token => token.IsDeleted, false)
            .Create();

        await _tokenCollection.InsertAsync(matchingToken);
        await _tokenCollection.InsertAsync(nonMatchingToken);

        var filters = TokenFilters.WithSpecifications()
            .WithType(type)
            .Build();

        // act: query filtered by token type
        var result = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);

        // assert: only the matching token should be returned
        Assert.Single(result);

        Assert.Equal(matchingToken.Type, result.First().Type);
        Assert.Equal(matchingToken.Id, result.First().Id);
    }


    [Fact(DisplayName = "[infrastructure] - when filtering tokens by user id, then only tokens from that user are returned")]
    public async Task WhenFilteringTokensByUserId_ThenOnlyTokensFromThatUserAreReturned()
    {
        /* arrange: create user and token with user id */
        var realm = _fixture.Create<Realm>();
        var userId = Identifier.Generate<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
            .With(token => token.Value, "some-user-token")
            .With(token => token.UserId, userId)
            .With(token => token.RealmId, realm.Id)
            .With(token => token.IsDeleted, false)
            .Create();

        await _tokenCollection.InsertAsync(token);

        var filters = TokenFilters.WithSpecifications()
            .WithUserId(userId)
            .WithValue(token.Value)
            .Build();

        /* act: query tokens filtered by user id and value */
        var result = await _tokenCollection.GetTokensAsync(filters, CancellationToken.None);
        var retrievedToken = result.FirstOrDefault();

        /* assert: only token with matching user id is returned */
        Assert.NotNull(retrievedToken);

        Assert.Equal(token.Id, retrievedToken.Id);
        Assert.Equal(token.Value, retrievedToken.Value);
        Assert.Equal(userId, retrievedToken.UserId);
    }

    [Fact(DisplayName = "[infrastructure] - when counting tokens with filters, then count must reflect filtered records")]
    public async Task WhenCountingTokensWithFilters_ThenCountMustReflectFilteredRecords()
    {
        /* arrange: create 10 tokens with varied realmId, userId and IsDeleted */
        var realm = _fixture.Create<Realm>();
        var realmId2 = Identifier.Generate<Realm>();

        _realmProvider.Setup(provider => provider.GetCurrentRealm())
            .Returns(realm);

        var userId1 = Identifier.Generate<User>();
        var userId2 = Identifier.Generate<User>();

        var tokens = new List<Domain.Aggregates.SecurityToken>();

        for (int index = 0; index < 10; index++)
        {
            var token = _fixture.Build<Domain.Aggregates.SecurityToken>()
                .With(token => token.RealmId, index < 5 ? realm.Id : realmId2) // first 5 realm1, last 5 realm2
                .With(token => token.UserId, index % 2 == 0 ? userId1 : userId2) // alternate user
                .With(token => token.Value, $"token{index}")
                .With(token => token.IsDeleted, index % 3 == 0) // every third token is deleted
                .Create();

            tokens.Add(token);

            await _tokenCollection.InsertAsync(token);
        }

        /* act: count tokens filtered by realm1 and IsDeleted = false */
        var filters = TokenFilters.WithSpecifications()
            .WithRealmId(realm.Id)
            .WithIsDeleted(false)
            .Build();

        var filteredCount = await _tokenCollection.CountAsync(filters);
        var expectedCount = tokens.Count(token => token.RealmId == realm.Id && !token.IsDeleted);

        /* assert: expected count of tokens for realm*/
        Assert.Equal(expectedCount, filteredCount);
    }

    #pragma warning disable S2325

    public async Task DisposeAsync() => await Task.CompletedTask;
    public async Task InitializeAsync()
    {
        await _mongoFixture.CleanDatabaseAsync();
    }
}
