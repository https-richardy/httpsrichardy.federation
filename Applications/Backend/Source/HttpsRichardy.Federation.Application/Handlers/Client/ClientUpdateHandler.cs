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

        var nameFilter = ClientFilters.WithSpecifications()
            .WithName(parameters.Name)
            .Build();

        var clientsWithSameName = await collection.GetClientsAsync(nameFilter, cancellation: cancellation);
        var existingClient = clientsWithSameName.FirstOrDefault(existing => existing.Id != parameters.Id);

        if (existingClient is not null)
        {
            return Result<ClientScheme>.Failure(ClientErrors.ClientAlreadyExists);
        }

        client = ClientMapper.AsClient(parameters, client);

        var updatedClient = await collection.UpdateAsync(client, cancellation: cancellation);

        return Result<ClientScheme>.Success(ClientMapper.AsResponse(updatedClient));
    }
}
