namespace HttpsRichardy.Federation.Infrastructure.Persistence;

public sealed class RealmCollection(IMongoDatabase database) :
    AggregateCollection<Realm>(database, Collections.Realms),
    IRealmCollection
{
    public async Task<IReadOnlyCollection<Realm>> GetRealmsAsync(RealmFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Realm>()
            .As<Realm, Realm, BsonDocument>()
            .FilterRealms(filters)
            .Paginate(filters.Pagination)
            .Sort(filters.Sort);

        var options = new AggregateOptions { AllowDiskUse = true };
        var aggregation = await _collection.AggregateAsync(pipeline, options, cancellation);

        var bsonDocuments = await aggregation.ToListAsync(cancellation);
        var realms = bsonDocuments
            .Select(bson => BsonSerializer.Deserialize<Realm>(bson))
            .ToList();

        return realms;
    }

    public async Task<long> CountAsync(RealmFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Realm>()
            .As<Realm, Realm, BsonDocument>()
            .FilterRealms(filters)
            .Count();

        var aggregation = await _collection.AggregateAsync(pipeline, cancellationToken: cancellation);
        var result = await aggregation.FirstOrDefaultAsync(cancellation);

        if (result is null)
            return 0;

        return result.Count;
    }
}
