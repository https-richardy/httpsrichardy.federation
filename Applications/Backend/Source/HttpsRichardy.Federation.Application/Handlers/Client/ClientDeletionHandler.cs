namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class ClientDeletionHandler(IClientCollection collection) : IDispatchHandler<ClientDeletionScheme, Result>
{
    public async Task<Result> HandleAsync(ClientDeletionScheme parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var clients = await collection.GetClientsAsync(filters, cancellation: cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result.Failure(ClientErrors.ClientDoesNotExist);
        }

        await collection.DeleteAsync(client, cancellation: cancellation);

        return Result.Success();
    }
}
