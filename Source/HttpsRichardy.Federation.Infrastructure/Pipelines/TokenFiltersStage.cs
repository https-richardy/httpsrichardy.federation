namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class TokenFiltersStage
{
    public static PipelineDefinition<SecurityToken, BsonDocument> FilterTokens(this PipelineDefinition<SecurityToken, BsonDocument> pipeline,
        TokenFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmptyEnum(Documents.SecurityToken.Type, filters.Type),
            FilterDefinitions.MatchIfNotEmpty(Documents.SecurityToken.Value, filters.Value),
            FilterDefinitions.MatchIfNotEmpty(Documents.SecurityToken.UserId, filters.UserId),

            FilterDefinitions.MatchIfNotEmpty(Documents.SecurityToken.RealmId, realm?.Id),
            FilterDefinitions.MatchBool(Documents.SecurityToken.IsDeleted, filters.IsDeleted)
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
