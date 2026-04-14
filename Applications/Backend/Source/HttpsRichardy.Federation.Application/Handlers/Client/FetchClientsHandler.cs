namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class FetchClientsHandler(IClientCollection clientCollection) :
    IDispatchHandler<ClientsFetchParameters, Result<Pagination<ClientScheme>>>
{
    public async Task<Result<Pagination<ClientScheme>>> HandleAsync(
        ClientsFetchParameters parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithName(parameters.Name)
            .WithClientId(parameters.ClientId)
            .WithSort(parameters.Sort)
            .WithPagination(parameters.Pagination)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, cancellation);
        var totalClients = await clientCollection.CountClientsAsync(filters, cancellation);

        var pagination = new Pagination<ClientScheme>
        {
            Items = [.. clients.Select(client => ClientMapper.AsResponse(client))],
            Total = (int)totalClients,
            PageNumber = parameters.Pagination?.PageNumber ?? 1,
            PageSize = parameters.Pagination?.PageSize ?? 20,
        };

        return Result<Pagination<ClientScheme>>.Success(pagination);
    }
}
