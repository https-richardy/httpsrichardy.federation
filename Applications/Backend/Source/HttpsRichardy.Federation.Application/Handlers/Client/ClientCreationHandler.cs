namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class ClientCreationHandler(IClientCredentialsGenerator credentialsGenerator, IRealmProvider realmProvider, IClientCollection clientCollection) :
    IDispatchHandler<ClientCreationScheme, Result>
{
    public async Task<Result> HandleAsync(ClientCreationScheme parameters, CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var credentials = await credentialsGenerator.GenerateAsync(parameters.Name, cancellation);

        var client = parameters.AsClient(credentials, realm);

        await clientCollection.InsertAsync(client, cancellation: cancellation);

        return Result.Success();
    }
}
