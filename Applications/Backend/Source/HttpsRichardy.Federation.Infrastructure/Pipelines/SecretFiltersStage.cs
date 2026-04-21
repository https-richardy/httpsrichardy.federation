namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class SecretFiltersStage
{
    public static PipelineDefinition<Secret, BsonDocument> FilterSecrets(this PipelineDefinition<Secret, BsonDocument> pipeline,
        SecretFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var now = filters.Now ?? DateTime.UtcNow;

        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Secret.Id, filters.Id),
            FilterDefinitions.MatchIfNotEmpty(Documents.Secret.RealmId, filters.RealmId ?? realm?.Id),
        };

        if (filters.CanSign is true)
        {
            definitions.Add(Builders<BsonDocument>.Filter.Or(
                    Builders<BsonDocument>.Filter.Eq(Documents.Secret.ExpiresAt, BsonNull.Value),
                    Builders<BsonDocument>.Filter.Gt(Documents.Secret.ExpiresAt, now)
            ));
        }

        // a secret is in the grace period if it has already expired for signing and its grace period has not ended yet.

        if (filters.InGracePeriod is true)
        {
            definitions.Add(Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Ne(Documents.Secret.ExpiresAt, BsonNull.Value),
                    Builders<BsonDocument>.Filter.Lte(Documents.Secret.ExpiresAt, now),

                    Builders<BsonDocument>.Filter.Ne(Documents.Secret.GracePeriodEndsAt, BsonNull.Value),
                    Builders<BsonDocument>.Filter.Gt(Documents.Secret.GracePeriodEndsAt, now)
                ));
        }

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
