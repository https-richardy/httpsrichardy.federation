namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class RealmFiltersStage
{
    public static PipelineDefinition<Realm, BsonDocument> FilterRealms(
        this PipelineDefinition<Realm, BsonDocument> pipeline, RealmFilters filters)
    {
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Realm.Name, filters.Name),
            FilterDefinitions.MatchIfNotEmpty(Documents.Realm.Id, filters.Id),
            FilterDefinitions.MatchBool(Documents.Realm.IsDeleted, filters.IsDeleted)
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
