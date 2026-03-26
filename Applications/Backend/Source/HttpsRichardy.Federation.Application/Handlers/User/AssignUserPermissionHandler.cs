namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class AssignUserPermissionHandler(IUserCollection userCollection, IPermissionCollection permissionCollection) :
    IDispatchHandler<AssignUserPermissionScheme, Result>
{
    public async Task<Result> HandleAsync(AssignUserPermissionScheme parameters, CancellationToken cancellation = default)
    {
        var userFilters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var permissionFilters = PermissionFilters.WithSpecifications()
            .WithName(parameters.PermissionName.ToLower())
            .Build();

        var users = await userCollection.GetUsersAsync(userFilters, cancellation: cancellation);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result.Failure(UserErrors.UserDoesNotExist);
        }

        var permissions = await permissionCollection.GetPermissionsAsync(permissionFilters, cancellation: cancellation);
        var existingPermission = permissions.FirstOrDefault();

        if (existingPermission is null)
        {
            return Result.Failure(PermissionErrors.PermissionDoesNotExist);
        }

        if (user.Permissions.Any(permission => permission.Name == existingPermission.Name))
        {
            return Result.Failure(UserErrors.UserAlreadyHasPermission);
        }

        user.Permissions.Add(existingPermission);

        await userCollection.UpdateAsync(user, cancellation: cancellation);

        return Result.Success();
    }
}