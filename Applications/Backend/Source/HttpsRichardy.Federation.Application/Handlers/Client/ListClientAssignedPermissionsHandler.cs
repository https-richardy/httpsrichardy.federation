namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class ListClientAssignedPermissionsHandler(IClientCollection collection) :
    IDispatchHandler<ListClientAssignedPermissionsParameters, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        ListClientAssignedPermissionsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var clients = await collection.GetClientsAsync(filters, cancellation);
        var client = clients.FirstOrDefault();

        return client is not null
            ? Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(client.Permissions))
            : Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(ClientErrors.ClientDoesNotExist);
    }
}
