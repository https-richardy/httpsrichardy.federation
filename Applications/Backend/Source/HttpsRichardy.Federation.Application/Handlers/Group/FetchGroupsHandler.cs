namespace HttpsRichardy.Federation.Application.Handlers.Group;

public sealed class FetchGroupsHandler(IGroupCollection collection) :
    IDispatchHandler<GroupsFetchParameters, Result<Pagination<GroupDetailsScheme>>>
{
    public async Task<Result<Pagination<GroupDetailsScheme>>> HandleAsync(
        GroupsFetchParameters parameters, CancellationToken cancellation = default)
    {
        var filters = GroupMapper.AsFilters(parameters);

        var groups = await collection.GetGroupsAsync(filters, cancellation: cancellation);
        var totalGroups = await collection.CountAsync(filters, cancellation: cancellation);

        var pagination = new Pagination<GroupDetailsScheme>
        {
            Items = [.. groups.Select(group => GroupMapper.AsResponse(group))],
            Total = (int)totalGroups,
            PageNumber = parameters.Pagination?.PageNumber ?? 1,
            PageSize = parameters.Pagination?.PageSize ?? 20
        };

        return Result<Pagination<GroupDetailsScheme>>.Success(pagination);
    }
}
