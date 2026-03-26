namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class ScopeFiltersStage
{
    public static PipelineDefinition<Scope, BsonDocument> FilterScopes(
        this PipelineDefinition<Scope, BsonDocument> pipeline, ScopeFilters filters)
    {
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Scope.Name, filters.Name),
            FilterDefinitions.MatchIfNotEmpty(Documents.Scope.Id, filters.Id),
            FilterDefinitions.MatchIfNotEmpty(Documents.Scope.RealmId, filters.RealmId),
            FilterDefinitions.MatchBool(Documents.Scope.IsDeleted, filters.IsDeleted)
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
