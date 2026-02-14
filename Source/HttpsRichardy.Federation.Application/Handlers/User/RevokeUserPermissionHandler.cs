namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class RevokeUserPermissionHandler(IUserCollection userCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<RevokeUserPermissionScheme, Result>
{
    public async Task<Result> HandleAsync(RevokeUserPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithIdentifier(parameters.PermissionId)
            .Build();

        var userFilters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var users = await userCollection.GetUsersAsync(userFilters, cancellation);
        var user = users.FirstOrDefault();

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation);
        var permission = permissions.FirstOrDefault();

        if (user is null)
        {
            return Result.Failure(UserErrors.UserDoesNotExist);
        }

        if (permission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        #pragma warning disable S1764

        // sonar suggests changing the lambda parameter name to avoid confusion,
        // but we prefer keeping it as 'permission' for readability. it does not interfere
        // with the outer variable because C# differentiates the lambda parameter from the argument.

        var permissionToRemove = user.Permissions.FirstOrDefault(permission => permission.Id == permission.Id);
        if (permissionToRemove is null)
        {
            return Result.Failure(UserErrors.PermissionNotAssigned);
        }

        user.Permissions.Remove(permissionToRemove);

        await userCollection.UpdateAsync(user, cancellation);

        return Result.Success();
    }
}