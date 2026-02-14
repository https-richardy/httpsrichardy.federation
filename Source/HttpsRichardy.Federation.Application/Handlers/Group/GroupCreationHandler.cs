namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class GroupCreationHandler(IGroupCollection groupCollection, IRealmProvider realmProvider) :
    IDispatchHandler<GroupCreationScheme, Result<GroupDetailsScheme>>
{
    public async Task<Result<GroupDetailsScheme>> HandleAsync(
        GroupCreationScheme parameters, CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var group = GroupMapper.AsGroup(parameters, realm);

        var filters = GroupFilters.WithSpecifications()
            .WithName(group.Name)
            .Build();

        var groups = await groupCollection.GetGroupsAsync(filters, cancellation: cancellation);
        var existingGroup = groups.FirstOrDefault();

        if (existingGroup is not null)
        {
            return Result<GroupDetailsScheme>.Failure(GroupErrors.GroupAlreadyExists);
        }

        await groupCollection.InsertAsync(group, cancellation: cancellation);

        return Result<GroupDetailsScheme>.Success(GroupMapper.AsResponse(group));
    }
}