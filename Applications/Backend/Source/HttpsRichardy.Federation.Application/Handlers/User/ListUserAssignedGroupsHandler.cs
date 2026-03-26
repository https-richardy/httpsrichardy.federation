namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class ListUserAssignedGroupsHandler(IUserCollection collection) :
    IDispatchHandler<ListUserAssignedGroupsParameters, Result<IReadOnlyCollection<GroupBasicDetailsScheme>>>
{
    public async Task<Result<IReadOnlyCollection<GroupBasicDetailsScheme>>> HandleAsync(
        ListUserAssignedGroupsParameters parameters, CancellationToken cancellation = default)
    {
        var filters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var users = await collection.GetUsersAsync(filters, cancellation);
        var user = users.FirstOrDefault();

        return user is not null
            ? Result<IReadOnlyCollection<GroupBasicDetailsScheme>>.Success(GroupMapper.AsBasicResponse(user.Groups))
            : Result<IReadOnlyCollection<GroupBasicDetailsScheme>>.Failure(UserErrors.UserDoesNotExist);
    }
}
