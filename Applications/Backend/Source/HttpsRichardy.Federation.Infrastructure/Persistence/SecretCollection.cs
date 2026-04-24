namespace HttpsRichardy.Federation.Infrastructure.Persistence;

public sealed class SecretCollection(IMongoDatabase database, IRealmProvider realmProvider) :
    AggregateCollection<Secret>(database, Collections.Secrets),
    ISecretCollection
{
    public async Task<IReadOnlyCollection<Secret>> GetSecretsAsync(
        SecretFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Secret>()
            .As<Secret, Secret, BsonDocument>()
            .FilterSecrets(filters, realmProvider)
            .Paginate(filters.Pagination)
            .Sort(filters.Sort);

        var options = new AggregateOptions { AllowDiskUse = true };
        var aggregation = await _collection.AggregateAsync(pipeline, options, cancellation);

        var bsonDocuments = await aggregation.ToListAsync(cancellation);
        var secrets = bsonDocuments
            .Select(bson => BsonSerializer.Deserialize<Secret>(bson))
            .ToList();

        return secrets;
    }

    public async Task<System.Numerics.BigInteger> CountSecretsAsync(
        SecretFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Secret>()
            .As<Secret, Secret, BsonDocument>()
            .FilterSecrets(filters, realmProvider)
            .Count();

        var aggregation = await _collection.AggregateAsync(pipeline, cancellationToken: cancellation);
        var result = await aggregation.FirstOrDefaultAsync(cancellation);

        return result?.Count ?? 0;
    }
}
