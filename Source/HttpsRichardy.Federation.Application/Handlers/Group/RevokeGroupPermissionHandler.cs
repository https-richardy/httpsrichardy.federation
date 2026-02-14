namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class RevokeGroupPermissionHandler(IGroupCollection groupCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<RevokeGroupPermissionScheme, Result>
{
    public async Task<Result> HandleAsync(RevokeGroupPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var groupFilters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var groups = await groupCollection.GetGroupsAsync(groupFilters, cancellation);
        var group = groups.FirstOrDefault();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation);
        var permission = permissions.FirstOrDefault();

        if (group is null)
        {
            return Result.Failure(GroupErrors.GroupDoesNotExist);
        }

        if (permission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        #pragma warning disable S1764

        // sonar suggests changing the lambda parameter name to avoid confusion,
        // but we prefer keeping it as 'permission' for readability. it does not interfere
        // with the outer variable because C# differentiates the lambda parameter from the argument.

        var permissionToRemove = group.Permissions.FirstOrDefault(permission => permission.Id == permission.Id);
        if (permissionToRemove is null)
        {
            return Result.Failure(GroupErrors.PermissionNotAssigned);
        }

        group.Permissions.Remove(permissionToRemove);

        await groupCollection.UpdateAsync(group, cancellation);

        return Result.Success();
    }
}