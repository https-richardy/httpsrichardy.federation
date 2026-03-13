namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class RealmCreationHandler(IRealmCollection collection, IClientCredentialsGenerator credentialsGenerator) :
    IDispatchHandler<RealmCreationScheme, Result<RealmDetailsScheme>>
{
    public async Task<Result<RealmDetailsScheme>> HandleAsync(
        RealmCreationScheme parameters, CancellationToken cancellation = default)
    {
        var filters = RealmFilters.WithSpecifications()
            .WithName(parameters.Name)
            .Build();

        var realms = await collection.GetRealmsAsync(filters, cancellation);
        if (realms.Count > 0)
        {
            return Result<RealmDetailsScheme>.Failure(RealmErrors.RealmAlreadyExists);
        }

        var credentials = await credentialsGenerator.GenerateAsync(parameters.Name, cancellation: cancellation);
        var realm = RealmMapper.AsRealm(
            realm: parameters,
            clientId: credentials.ClientId,
            secretHash: credentials.ClientSecret
        );

        var masterFilters = RealmFilters.WithSpecifications()
            .WithName("master")
            .Build();

        var matchingRealms = await collection.GetRealmsAsync(masterFilters, cancellation);
        var defaultRealm = matchingRealms.FirstOrDefault()!;

        realm.Permissions = defaultRealm.Permissions
            .Where(permission => RealmPermissions.InitialPermissions.Contains(permission.Name))
            .ToList();

        await collection.InsertAsync(realm, cancellation: cancellation);

        return Result<RealmDetailsScheme>.Success(RealmMapper.AsResponse(realm));
    }
}
