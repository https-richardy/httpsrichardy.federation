namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class ClientCreationHandler(IClientCredentialsGenerator credentialsGenerator, IRealmProvider realmProvider, IClientCollection clientCollection) :
    IDispatchHandler<ClientCreationScheme, Result<ClientScheme>>
{
    public async Task<Result<ClientScheme>> HandleAsync(ClientCreationScheme parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithName(parameters.Name)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, cancellation: cancellation);
        var existingClient = clients.FirstOrDefault();

        if (existingClient is not null)
        {
            return Result<ClientScheme>.Failure(ClientErrors.ClientAlreadyExists);
        }

        var realm = realmProvider.GetCurrentRealm();
        var credentials = await credentialsGenerator.GenerateAsync(parameters.Name, cancellation);

        var client = parameters.AsClient(credentials, realm);

        await clientCollection.InsertAsync(client, cancellation: cancellation);

        return Result<ClientScheme>.Success(client.AsResponse());
    }
}
