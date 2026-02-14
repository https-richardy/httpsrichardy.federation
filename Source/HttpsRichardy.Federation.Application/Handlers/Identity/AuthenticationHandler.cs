namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class AuthenticationHandler(IAuthenticationService authenticationService) :
    IDispatchHandler<AuthenticationCredentials, Result<AuthenticationResult>>
{
    public async Task<Result<AuthenticationResult>> HandleAsync(AuthenticationCredentials credentials, CancellationToken cancellation = default)
    {
        return await authenticationService.AuthenticateAsync(credentials, cancellation: cancellation);
    }
}