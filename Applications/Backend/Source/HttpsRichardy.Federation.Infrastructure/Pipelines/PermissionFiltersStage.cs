namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class PermissionFiltersStage
{
    public static PipelineDefinition<Permission, BsonDocument> FilterPermissions(this PipelineDefinition<Permission, BsonDocument> pipeline,
        PermissionFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Permission.RealmId, realm?.Id),
            FilterDefinitions.MatchIfNotEmpty(Documents.Permission.Id, filters.Id),
            FilterDefinitions.MatchIfNotEmpty(Documents.Permission.Name, filters.Name),
            FilterDefinitions.MatchBool(Documents.Permission.IsDeleted, filters.IsDeleted),
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}
