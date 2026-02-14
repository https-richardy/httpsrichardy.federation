namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class GroupUpdateHandler(IGroupCollection collection) :
    IDispatchHandler<GroupUpdateScheme, Result<GroupDetailsScheme>>
{
    public async Task<Result<GroupDetailsScheme>> HandleAsync(
        GroupUpdateScheme parameters, CancellationToken cancellation = default)
    {
        var filters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var groups = await collection.GetGroupsAsync(filters, cancellation: cancellation);
        var group = groups.FirstOrDefault();

        if (group is null)
        {
            return Result<GroupDetailsScheme>.Failure(GroupErrors.GroupDoesNotExist);
        }

        group = GroupMapper.AsGroup(parameters, group);

        var updatedGroup = await collection.UpdateAsync(group, cancellation: cancellation);

        return Result<GroupDetailsScheme>.Success(GroupMapper.AsResponse(updatedGroup));
    }
}
