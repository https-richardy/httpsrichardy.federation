namespace HttpsRichardy.Federation.Application.Handlers.Connect;

public sealed class FetchOpenIDConfigurationHandler(IHostInformationProvider host, IRealmCollection realmCollection) :
    IDispatchHandler<FetchOpenIDConfigurationParameters, Result<OpenIDConfigurationScheme>>
{
    public async Task<Result<OpenIDConfigurationScheme>> HandleAsync(
        FetchOpenIDConfigurationParameters parameters, CancellationToken cancellation = default)
    {
        var realmFilters = RealmFilters.WithSpecifications()
            .WithName(parameters.Realm)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<OpenIDConfigurationScheme>.Failure(RealmErrors.RealmDoesNotExist);
        }

        var configuration = ConnectMapper.AsConfiguration(host.Address, parameters.Realm);

        return Result<OpenIDConfigurationScheme>.Success(configuration);
    }
}
