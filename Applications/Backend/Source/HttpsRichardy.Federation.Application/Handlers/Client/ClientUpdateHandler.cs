namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class ClientUpdateHandler(IClientCollection collection) :
    IDispatchHandler<ClientUpdateScheme, Result<ClientScheme>>
{
    public async Task<Result<ClientScheme>> HandleAsync(
        ClientUpdateScheme parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var clients = await collection.GetClientsAsync(filters, cancellation: cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result<ClientScheme>.Failure(ClientErrors.ClientDoesNotExist);
        }

        client = ClientMapper.AsClient(parameters, client);

        var updatedClient = await collection.UpdateAsync(client, cancellation: cancellation);

        return Result<ClientScheme>.Success(ClientMapper.AsResponse(updatedClient));
    }
}
