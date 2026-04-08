namespace HttpsRichardy.Federation.Infrastructure.Pipelines;

public static class ClientFiltersStage
{
    public static PipelineDefinition<Client, BsonDocument> FilterClients(this PipelineDefinition<Client, BsonDocument> pipeline,
        ClientFilters filters, IRealmProvider realmProvider)
    {
        var realm = realmProvider.GetCurrentRealm();
        var definitions = new List<FilterDefinition<BsonDocument>>
        {
            FilterDefinitions.MatchIfNotEmpty(Documents.Client.Name, filters.Name),
            FilterDefinitions.MatchIfNotEmpty(Documents.Client.RealmId, realm?.Id),
            FilterDefinitions.MatchBool(Documents.Client.IsDeleted, filters.IsDeleted)
        };

        return pipeline.Match(Builders<BsonDocument>.Filter.And(definitions));
    }
}