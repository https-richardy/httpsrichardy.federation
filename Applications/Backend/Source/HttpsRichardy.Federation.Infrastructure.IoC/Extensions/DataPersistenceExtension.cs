namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class DataPersistenceExtension
{
    public static void AddDataPersistence(this IServiceCollection services, ISettings settings)
    {
        services.AddSingleton<IMongoDatabase>(provider =>
        {
            var mongoClient = new MongoClient(settings.Database.ConnectionString);
            var database = mongoClient.GetDatabase(settings.Database.DatabaseName);

            return database;
        });

        services.AddTransient<IUserCollection, UserCollection>();
        services.AddTransient<IPermissionCollection, PermissionCollection>();
        services.AddTransient<IGroupCollection, GroupCollection>();
        services.AddTransient<IScopeCollection, ScopesCollection>();
        services.AddTransient<ITokenCollection, TokenCollection>();
        services.AddTransient<IRealmCollection, RealmCollection>();
        services.AddTransient<ISecretCollection, SecretCollection>();
    }
}
