namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class IdentityEnrollmentHandler(
    IUserCollection userCollection,
    IPasswordHasher passwordHasher,
    IRealmProvider realmProvider
) : IDispatchHandler<IdentityEnrollmentCredentials, Result<UserDetailsScheme>>
{
    public async Task<Result<UserDetailsScheme>> HandleAsync(IdentityEnrollmentCredentials parameters, CancellationToken cancellation = default)
    {
        var filters = UserFilters.WithSpecifications()
            .WithUsername(parameters.Username)
            .Build();

        var users = await userCollection.GetUsersAsync(filters, cancellation);
        var user = users.FirstOrDefault();

        if (user is not null)
        {
            return Result<UserDetailsScheme>.Failure(IdentityErrors.UserAlreadyExists);
        }

        var realm = realmProvider.GetCurrentRealm();
        var identity = UserMapper.AsUser(parameters, realm.Id);

        identity.PasswordHash = await passwordHasher.HashPasswordAsync(parameters.Password);

        await userCollection.InsertAsync(identity, cancellation: cancellation);

        return Result<UserDetailsScheme>.Success(UserMapper.AsResponse(identity));
    }
}