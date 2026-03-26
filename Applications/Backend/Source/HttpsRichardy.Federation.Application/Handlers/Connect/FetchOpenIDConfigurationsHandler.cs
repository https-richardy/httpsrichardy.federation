namespace HttpsRichardy.Federation.Application.Handlers.Connect;

public sealed class FetchOpenIDConfigurationHandler(IHostInformationProvider host) :
    IDispatchHandler<FetchOpenIDConfigurationParameters, Result<OpenIDConfigurationScheme>>
{
    public Task<Result<OpenIDConfigurationScheme>> HandleAsync(
        FetchOpenIDConfigurationParameters parameters, CancellationToken cancellation = default)
    {
        var configuration = ConnectMapper.AsConfiguration(host.Address);

        return Task.FromResult(Result<OpenIDConfigurationScheme>.Success(configuration));
    }
}
