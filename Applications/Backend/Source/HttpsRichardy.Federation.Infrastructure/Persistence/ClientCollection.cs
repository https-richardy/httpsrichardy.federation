namespace HttpsRichardy.Federation.Infrastructure.Persistence;

public sealed class ClientCollection(IMongoDatabase database, IRealmProvider realmProvider) :
    AggregateCollection<Client>(database, Collections.Clients), IClientCollection
{
    public async Task<IReadOnlyCollection<Client>> GetClientsAsync(
        ClientFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Client>()
            .As<Client, Client, BsonDocument>()
            .FilterClients(filters, realmProvider)
            .Paginate(filters.Pagination)
            .Sort(filters.Sort);

        var options = new AggregateOptions { AllowDiskUse = true };
        var aggregation = await _collection.AggregateAsync(pipeline, options, cancellation);

        var bsonDocuments = await aggregation.ToListAsync(cancellation);
        var clients = bsonDocuments
            .Select(bson => BsonSerializer.Deserialize<Client>(bson))
            .ToList();

        return clients;
    }

    public async Task<System.Numerics.BigInteger> CountClientsAsync(
        ClientFilters filters, CancellationToken cancellation = default)
    {
        var pipeline = PipelineDefinitionBuilder
            .For<Client>()
            .As<Client, Client, BsonDocument>()
            .FilterClients(filters, realmProvider)
            .Count();

        var aggregation = await _collection.AggregateAsync(pipeline, cancellationToken: cancellation);
        var result = await aggregation.FirstOrDefaultAsync(cancellation);

        return result?.Count ?? 0;
    }
}