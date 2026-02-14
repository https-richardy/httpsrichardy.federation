namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class ListUserAssignedPermissionsHandler(IUserCollection collection) :
    IDispatchHandler<ListUserAssignedPermissionsParameters, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        ListUserAssignedPermissionsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var users = await collection.GetUsersAsync(filters, cancellation);
        var user = users.FirstOrDefault();

        return user is not null
            ? Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(user.Permissions))
            : Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(UserErrors.UserDoesNotExist);
    }
}
