namespace HttpsRichardy.Federation.Application.Handlers.Permission;

public sealed class FetchPermissionsHandler(IPermissionCollection collection) :
    IDispatchHandler<PermissionsFetchParameters, Result<Pagination<PermissionDetailsScheme>>>
{
    public async Task<Result<Pagination<PermissionDetailsScheme>>> HandleAsync(
        PermissionsFetchParameters parameters, CancellationToken cancellation = default)
    {
        var filters = PermissionMapper.AsFilters(parameters);

        var permissions = await collection.GetPermissionsAsync(filters, cancellation: cancellation);
        var totalPermission = await collection.CountAsync(filters, cancellation: cancellation);

        var pagination = new Pagination<PermissionDetailsScheme>
        {
            Items = [.. permissions.Select(permission => PermissionMapper.AsResponse(permission))],
            Total = (int)totalPermission,
            PageNumber = parameters.Pagination?.PageNumber ?? 1,
            PageSize = parameters.Pagination?.PageSize ?? 20
        };

        return Result<Pagination<PermissionDetailsScheme>>.Success(pagination);
    }
}
