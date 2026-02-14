namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class AssignUserToGroupHandler(IUserCollection userCollection, IGroupCollection groupCollection) :
    IDispatchHandler<AssignUserToGroupScheme, Result>
{
    public async Task<Result> HandleAsync(AssignUserToGroupScheme parameters, CancellationToken cancellation = default)
    {
        var userFilters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var matchingUsers = await userCollection.GetUsersAsync(userFilters, cancellation);
        var existingUser = matchingUsers.FirstOrDefault();

        if (existingUser is null)
        {
            return Result.Failure(UserErrors.UserDoesNotExist);
        }

        var groupFilters = GroupFilters.WithSpecifications()
            .WithIdentifier(parameters.GroupId)
            .Build();

        var matchingGroups = await groupCollection.GetGroupsAsync(groupFilters, cancellation);
        var existingGroup = matchingGroups.FirstOrDefault();

        if (existingGroup is null)
        {
            return Result.Failure(GroupErrors.GroupDoesNotExist);
        }

        if (existingUser.Groups.Any(group => group.Id == parameters.GroupId))
        {
            return Result.Failure(UserErrors.UserAlreadyInGroup);
        }

        existingUser.Groups.Add(existingGroup);

        await userCollection.UpdateAsync(existingUser, cancellation);

        return Result.Success();
    }
}
