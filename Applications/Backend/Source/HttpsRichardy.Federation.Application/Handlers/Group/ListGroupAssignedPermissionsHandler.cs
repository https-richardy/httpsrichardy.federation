namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class ListGroupAssignedPermissionsHandler(IGroupCollection collection) :
    IDispatchHandler<ListGroupAssignedPermissionsParameters, Result<IReadOnlyCollection<PermissionDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<PermissionDetailsScheme>>> HandleAsync(
        ListGroupAssignedPermissionsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var groups = await collection.GetGroupsAsync(filters, cancellation);
        var group = groups.FirstOrDefault();

        return group is not null
            ? Result<IReadOnlyCollection<PermissionDetailsScheme>>.Success(PermissionMapper.AsResponse(group.Permissions))
            : Result<IReadOnlyCollection<PermissionDetailsScheme>>.Failure(GroupErrors.GroupDoesNotExist);
    }
}
