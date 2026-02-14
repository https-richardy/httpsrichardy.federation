namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class UserFiltersStage
{
    public static PipelineDefinition<User, BsonDocument> FilterUsers(this PipelineDefinition<User, BsonDocument> pipeline,
        UserFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.User.Id, filters.Id),
            FilterDefinitions.MatchIfNotEmpty(Documents.User.Username, filters.Username),
            FilterDefinitions.MatchIfNotEmpty(Documents.User.RealmId, realm?.Id),
            FilterDefinitions.MatchBool(Documents.User.IsDeleted, filters.IsDeleted)
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
