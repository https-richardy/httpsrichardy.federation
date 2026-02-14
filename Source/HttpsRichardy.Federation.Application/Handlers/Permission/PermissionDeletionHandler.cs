namespace HttpsRichardy.Federation.Application.Handlers.Permission;

public sealed class PermissionDeletionHandler(IPermissionCollection collection) : IDispatchHandler<PermissionDeletionScheme, Result>
{
    public async Task<Result> HandleAsync(PermissionDeletionScheme parameters, CancellationToken cancellation = default)
    {
        var filters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var permissions = await collection.GetPermissionsAsync(filters, cancellation: cancellation);
        var permission = permissions.FirstOrDefault();

        if (permission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        await collection.DeleteAsync(permission, cancellation: cancellation);

        return Result.Success();
    }
}