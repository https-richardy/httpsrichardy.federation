namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class IndexesExtension
{
    public static void EnsureIndexes(this IMongoDatabase database)
    {
        #pragma warning disable IDE0055

        var userCollection =        database.GetCollection<User>("federation.users");
        var permissionCollection =  database.GetCollection<Permission>("federation.permissions");
        var groupCollection =       database.GetCollection<Group>("federation.groups");

        var tokenCollection =       database.GetCollection<SecurityToken>("federation.tokens");
        var realmCollection =       database.GetCollection<Realm>("federation.realms");
        var clientCollection =      database.GetCollection<Client>("federation.clients");

        var userIndexes = new[]
        {
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(user => user.Username)),
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(user => user.IsDeleted)),
            new CreateIndexModel<User>(Builders<User>.IndexKeys
                .Ascending(user => user.Username)
                .Ascending(user => user.RealmId))
        };

        var groupIndexes = new[]
        {
            new CreateIndexModel<Group>(Builders<Group>.IndexKeys.Ascending(group => group.Name)),
            new CreateIndexModel<Group>(Builders<Group>.IndexKeys.Ascending(group => group.RealmId)),
            new CreateIndexModel<Group>(Builders<Group>.IndexKeys
                .Ascending(group => group.RealmId)
                .Ascending(group => group.Name))
        };

        var permissionIndexes = new[]
        {
            new CreateIndexModel<Permission>(Builders<Permission>.IndexKeys.Ascending(permission => permission.Name)),
            new CreateIndexModel<Permission>(Builders<Permission>.IndexKeys.Ascending(permission => permission.RealmId)),
            new CreateIndexModel<Permission>(Builders<Permission>.IndexKeys
                .Ascending(permission => permission.RealmId)
                .Ascending(permission => permission.Name))
        };

        var tokenIndexes = new[]
        {
            new CreateIndexModel<SecurityToken>(Builders<SecurityToken>.IndexKeys.Ascending(token => token.UserId)),
            new CreateIndexModel<SecurityToken>(Builders<SecurityToken>.IndexKeys.Ascending(token => token.RealmId)),
            new CreateIndexModel<SecurityToken>(Builders<SecurityToken>.IndexKeys
                .Ascending(token => token.UserId)
                .Ascending(token => token.RealmId))
        };

        var realmIndexes = new[]
        {
            new CreateIndexModel<Realm>(Builders<Realm>.IndexKeys.Ascending(realm => realm.Name))
        };

        var clientIndexes = new[]
        {
            new CreateIndexModel<Client>(Builders<Client>.IndexKeys.Ascending(client => client.RealmId))
        };

        userCollection.Indexes.CreateMany(userIndexes);
        permissionCollection.Indexes.CreateMany(permissionIndexes);
        groupCollection.Indexes.CreateMany(groupIndexes);
        tokenCollection.Indexes.CreateMany(tokenIndexes);
        realmCollection.Indexes.CreateMany(realmIndexes);
        clientCollection.Indexes.CreateMany(clientIndexes);
    }
}
