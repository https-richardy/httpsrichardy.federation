namespace HttpsRichardy.Federation.Infrastructure.Persistence;

public sealed class SecretCollection(IMongoDatabase database) :
    AggregateCollection<Secret>(database, Collections.Secrets),
    ISecretCollection
{
    public async Task<Secret> GetSecretAsync(CancellationToken cancellation = default)
    {
        return await _collection
            .Find(Builders<Secret>.Filter.Empty)
            .FirstOrDefaultAsync(cancellation);
    }
}