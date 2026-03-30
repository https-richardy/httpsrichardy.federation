namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class BootstrapperExtension
{
    public static async Task UseBootstrapperAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();

        var realmCollection = scope.ServiceProvider.GetRequiredService<IRealmCollection>();
        var userCollection = scope.ServiceProvider.GetRequiredService<IUserCollection>();
        var permissionCollection = scope.ServiceProvider.GetRequiredService<IPermissionCollection>();

        var realmProvider = scope.ServiceProvider.GetRequiredService<IRealmProvider>();
        var credentialsGenerator = scope.ServiceProvider.GetRequiredService<IClientCredentialsGenerator>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var settings = scope.ServiceProvider.GetRequiredService<ISettings>();

        var realmCredentials = await credentialsGenerator.GenerateAsync("master", cancellation: default);

        var defaultRealm = new Realm { Name = "master", ClientId = realmCredentials.ClientId };
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation: default);
        var realm = realms.FirstOrDefault();

        if (realm is not null)
        {
            return;
        }

        defaultRealm.SecretHash = await passwordHasher.HashPasswordAsync(realmCredentials.ClientId + defaultRealm.Name);
        defaultRealm.Permissions = [.. RealmPermissions.SystemPermissions.Select(permissionName => new Permission
        {
            Id = Identifier.Generate<Permission>(),
            Name = permissionName,
            RealmId = defaultRealm.Id
        })];

        realmProvider.SetRealm(defaultRealm);

        await realmCollection.InsertAsync(defaultRealm);
        await permissionCollection.InsertManyAsync(defaultRealm.Permissions);

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
