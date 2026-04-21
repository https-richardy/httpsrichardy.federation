namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class RevokeClientPermissionHandler(IClientCollection clientCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<RevokeClientPermissionScheme, Result>
{
    public async Task<Result> HandleAsync(RevokeClientPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var clientFilters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, cancellation);
        var client = clients.FirstOrDefault();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation);
        var permission = permissions.FirstOrDefault();

        if (client is null)
        {
            return Result.Failure(ClientErrors.ClientDoesNotExist);
        }

        if (permission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        var permissionToRemove = client.Permissions.FirstOrDefault(where => where.Id == permission.Id);
        if (permissionToRemove is null)
        {
            return Result.Failure(ClientErrors.PermissionNotAssigned);
        }

        client.Permissions.Remove(permissionToRemove);

        await clientCollection.UpdateAsync(client, cancellation);

        return Result.Success();
    }
}
