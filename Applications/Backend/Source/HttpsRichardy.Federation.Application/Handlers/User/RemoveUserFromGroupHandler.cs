namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class RemoveUserFromGroupHandler(IUserCollection userCollection, IGroupCollection groupCollection) :
    IDispatchHandler<RemoveUserFromGroupScheme, Result>
{
    public async Task<Result> HandleAsync(RemoveUserFromGroupScheme parameters, CancellationToken cancellation = default)
    {
        var userFilters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var groupFilters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var users = await userCollection.GetUsersAsync(userFilters, cancellation);
        var user = users.FirstOrDefault();

        var groups = await groupCollection.GetGroupsAsync(groupFilters, cancellation);
        var group = groups.FirstOrDefault();

        if (user is null)
        {
            return Result.Failure(UserErrors.UserDoesNotExist);
        }

        if (group is null)
        {
            return Result.Failure(GroupErrors.GroupDoesNotExist);
        }

        #pragma warning disable S1764

        // sonar suggests changing the lambda parameter name to avoid confusion,
        // but we prefer keeping it as 'group' for readability. it does not interfere
        // with the outer variable because C# differentiates the lambda parameter from the argument.

        var groupToRemove = user.Groups.FirstOrDefault(group => group.Id == group.Id);
        if (groupToRemove is null)
        {
            return Result.Failure(UserErrors.UserNotInGroup);
        }

        user.Groups.Remove(groupToRemove);

        await userCollection.UpdateAsync(user, cancellation);

        return Result.Success();
    }
}
