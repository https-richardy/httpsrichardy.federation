namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class AssignPermissionToGroupHandler(IGroupCollection groupCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<AssignGroupPermissionScheme, Result<GroupDetailsScheme>>
{
    public async Task<Result<GroupDetailsScheme>> HandleAsync(
        AssignGroupPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var groupFilters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(parameters.PermissionName.ToLower())
            .Build();

        var groups = await groupCollection.GetGroupsAsync(groupFilters, cancellation: cancellation);
        var group = groups.FirstOrDefault();

        if (group is null)
        {
            return Result<GroupDetailsScheme>.Failure(GroupErrors.GroupDoesNotExist);
        }

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation: cancellation);
        var existingPermission = permissions.FirstOrDefault();

        if (existingPermission is null)
        {
            return Result<GroupDetailsScheme>.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        if (group.Permissions.Any(permission => permission.Name == existingPermission.Name))
        {
            return Result<GroupDetailsScheme>.Failure(GroupErrors.GroupAlreadyHasPermission);
        }

        group.Permissions.Add(existingPermission);

        await groupCollection.UpdateAsync(group, cancellation: cancellation);

        return Result<GroupDetailsScheme>.Success(GroupMapper.AsResponse(group));
    }
}