namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class GroupDeletionHandler(IGroupCollection collection) : IDispatchHandler<GroupDeletionScheme, Result>
{
    public async Task<Result> HandleAsync(GroupDeletionScheme parameters, CancellationToken cancellation = default)
    {
        var filters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var groups = await collection.GetGroupsAsync(filters, cancellation: cancellation);
        var group = groups.FirstOrDefault();

        if (group is null)
        {
            return Result.Failure(GroupErrors.GroupDoesNotExist);
        }

        await collection.DeleteAsync(group, cancellation: cancellation);

        return Result.Success();
    }
}