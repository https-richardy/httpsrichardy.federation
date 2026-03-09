namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class GroupFiltersStage
{
    public static PipelineDefinition<Group, BsonDocument> FilterGroups(this PipelineDefinition<Group, BsonDocument> pipeline,
        GroupFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Group.Id, filters.Id),
            FilterDefinitions.MustBeInIfNotEmpty(Documents.Group.Id, filters.Identifiers),
            FilterDefinitions.MatchIfNotEmpty(Documents.Group.Name, filters.Name),
            FilterDefinitions.MatchIfNotEmpty(Documents.Group.RealmId, realm?.Id),
            FilterDefinitions.MatchBool(Documents.Group.IsDeleted, filters.IsDeleted),
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
