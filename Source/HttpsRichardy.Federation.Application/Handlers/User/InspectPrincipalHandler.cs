namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class InspectPrincipalHandler(IPrincipalProvider principalProvider) :
    IDispatchHandler<InspectPrincipalParameters, Result<PrincipalDetailsScheme>>
{
    public Task<Result<PrincipalDetailsScheme>> HandleAsync(
        InspectPrincipalParameters parameters, CancellationToken cancellation = default)
    {
        var user = principalProvider.GetCurrentPrincipal();

        return user is not null
            ? Task.FromResult(Result<PrincipalDetailsScheme>.Success(UserMapper.AsPrincipal(user)))
            : Task.FromResult(Result<PrincipalDetailsScheme>.Failure(UserErrors.UserDoesNotExist));
    }
}
