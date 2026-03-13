namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class BootstrapperExtension
{
    public static async Task UseBootstrapperAsync(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();

        var realmCollection = scope.ServiceProvider.GetRequiredService<IRealmCollection>();
        var userCollection = scope.ServiceProvider.GetRequiredService<IUserCollection>();
        var scopeRepository = scope.ServiceProvider.GetRequiredService<IScopeCollection>();
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

        var scopes = new List<Scope>
        {
            new() { Id = Identifier.Generate<Scope>(), Name = Scopes.OpenID.Name,  Description = Scopes.OpenID.Description,  IsGlobal = true },
            new() { Id = Identifier.Generate<Scope>(), Name = Scopes.Profile.Name, Description = Scopes.Profile.Description, IsGlobal = true },
            new() { Id = Identifier.Generate<Scope>(), Name = Scopes.Email.Name,   Description = Scopes.Email.Description,   IsGlobal = true },
            new() { Id = Identifier.Generate<Scope>(), Name = Scopes.Address.Name, Description = Scopes.Address.Description, IsGlobal = true },
            new() { Id = Identifier.Generate<Scope>(), Name = Scopes.Phone.Name,   Description = Scopes.Phone.Description,   IsGlobal = true },
        };

        realmProvider.SetRealm(defaultRealm);

        await realmCollection.InsertAsync(defaultRealm);
        await scopeRepository.InsertManyAsync(scopes);
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
