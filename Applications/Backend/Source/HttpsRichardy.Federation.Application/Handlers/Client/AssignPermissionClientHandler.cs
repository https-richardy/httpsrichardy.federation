namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class AssignPermissionClientHandler(IClientCollection clientCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<AssignClientPermissionScheme, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        AssignClientPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var clientFilters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(parameters.PermissionName.ToLower())
            .Build();

        var clients = await clientCollection.GetClientsAsync(clientFilters, cancellation: cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(ClientErrors.ClientDoesNotExist);
        }

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation: cancellation);
        var existingPermission = permissions.FirstOrDefault();

        if (existingPermission is null)
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        if (client.Permissions.Any(permission => permission.Name == existingPermission.Name))
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(ClientErrors.ClientAlreadyHasPermission);
        }

        client.Permissions.Add(existingPermission);

        await clientCollection.UpdateAsync(client, cancellation: cancellation);

        return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(client.Permissions));
    }
}
