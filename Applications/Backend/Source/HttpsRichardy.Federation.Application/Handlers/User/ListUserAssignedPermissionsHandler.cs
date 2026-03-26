namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class ListUserAssignedPermissionsHandler(IUserCollection collection, IGroupCollection groupCollection) :
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

        if (user is null)
        {
            return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(UserErrors.UserDoesNotExist);
        }

        var identifiers = user.Groups
            .Select(group => group.Id)
            .ToList();

        var groupFilters = GroupFilters.WithSpecifications()
            .WithIdentifiers([.. identifiers])
            .Build();

        var groups = await groupCollection.GetGroupsAsync(groupFilters, cancellation);
        var permissions = groups
            .SelectMany(group => group.Permissions)
            .Concat(user.Permissions)
            .DistinctBy(permission => permission.Name)
            .ToList();

        return Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(permissions));
    }
}
