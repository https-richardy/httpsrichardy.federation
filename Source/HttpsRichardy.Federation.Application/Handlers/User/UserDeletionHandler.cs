namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class UserDeletionHandler(IUserCollection collection) : IDispatchHandler<UserDeletionScheme, Result>
{
    public async Task<Result> HandleAsync(UserDeletionScheme parameters, CancellationToken cancellation = default)
    {
        var filters = UserFilters.WithSpecifications()
            .WithIdentifier(parameters.UserId)
            .Build();

        var users = await collection.GetUsersAsync(filters, cancellation: cancellation);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result.Failure(UserErrors.UserDoesNotExist);
        }

        await collection.DeleteAsync(user, cancellation: cancellation);

        return Result.Success();
    }
}