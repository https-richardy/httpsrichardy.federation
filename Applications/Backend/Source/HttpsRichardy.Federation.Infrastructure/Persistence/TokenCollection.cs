namespace HttpsRichardy.Federation.Infrastructure.Persistence;

public sealed class TokenCollection(IMongoDatabase database, IRealmProvider realmProvider) :
    AggregateCollection<SecurityToken>(database, Collections.Tokens),
    ITokenCollection
{
    public async Task<IReadOnlyCollection<SecurityToken>> GetTokensAsync(TokenFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<SecurityToken>()
            .As<SecurityToken, SecurityToken, BsonDocument>()
            .FilterTokens(filters, realmProvider)
            .Paginate(filters.Pagination)
            .Sort(filters.Sort);

        var options = new AggregateOptions { AllowDiskUse = true };
        var aggregation = await _collection.AggregateAsync(pipeline, options, cancellation);

        var bsonDocuments = await aggregation.ToListAsync(cancellation);
        var tokens = bsonDocuments
            .Select(bson => BsonSerializer.Deserialize<SecurityToken>(bson))
            .ToList();

        return tokens;
    }

    public async Task<long> CountAsync(TokenFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<SecurityToken>()
            .As<SecurityToken, SecurityToken, BsonDocument>()
            .FilterTokens(filters, realmProvider)
            .Count();

        var aggregation = await _collection.AggregateAsync(pipeline, cancellationToken: cancellation);
        var result = await aggregation.FirstOrDefaultAsync(cancellation);

        if (result == null)
        {
            return 0;
        }

        return result.Count;
    }
}