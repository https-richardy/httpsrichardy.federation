namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class BootstrapperExtension
{
    public static async Task UseBootstrapperAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();

        var realmCollection = scope.ServiceProvider.GetRequiredService<IRealmCollection>();
        var clientCollection = scope.ServiceProvider.GetRequiredService<IClientCollection>();
        var userCollection = scope.ServiceProvider.GetRequiredService<IUserCollection>();
        var permissionCollection = scope.ServiceProvider.GetRequiredService<IPermissionCollection>();

        var realmProvider = scope.ServiceProvider.GetRequiredService<IRealmProvider>();
        var credentialsGenerator = scope.ServiceProvider.GetRequiredService<IClientCredentialsGenerator>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var settings = scope.ServiceProvider.GetRequiredService<ISettings>();

        var clientCredentials = await credentialsGenerator.GenerateAsync("root", cancellation: default);

        var defaultRealm = new Realm { Name = "master" };
        var defaultClient = new Client { Name = "root", Flows = [Grant.ClientCredentials] };

        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation: default);
        var realm = realms.FirstOrDefault();

        if (realm is not null)
        {
            return;
        }

        defaultRealm.Permissions = [.. RealmPermissions.SystemPermissions.Select(permissionName => new Permission
        {
            Id = Identifier.Generate<Permission>(),
            Name = permissionName,
            RealmId = defaultRealm.Id
        })];

        defaultClient.Secret = await passwordHasher.HashPasswordAsync(clientCredentials.ClientId + defaultRealm.Name);
        defaultClient.Permissions = [.. defaultRealm.Permissions];

        defaultRealm.Clients = [defaultClient];

        realmProvider.SetRealm(defaultRealm);

        await realmCollection.InsertAsync(defaultRealm);
        await permissionCollection.InsertManyAsync(defaultRealm.Permissions);
        await clientCollection.InsertAsync(defaultClient);

        var userFilters = UserFilters.WithSpecifications()
            .WithUsername(settings.Administration.Username)
            .Build();

        var existingUsers = await userCollection.GetUsersAsync(userFilters);
        var rootUser = existingUsers.FirstOrDefault();

        if (rootUser is null)
        {
            rootUser = new User
            {
                Username = settings.Administration.Username,
                RealmId = defaultRealm.Id,
                Permissions = [.. defaultRealm.Permissions],
                PasswordHash = await passwordHasher.HashPasswordAsync(settings.Administration.Password)
            };

            await userCollection.InsertAsync(rootUser);
        }
    }
}
