namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class AuthenticationService(IUserCollection userCollection, IPasswordHasher passwordHasher, ISecurityTokenService tokenService) : IAuthenticationService
{
    public async Task<Result<AuthenticationResult>> AuthenticateAsync(AuthenticationCredentials credentials, CancellationToken cancellation = default)
    {
        var filters = new UserFiltersBuilder()
            .WithUsername(credentials.Username)
            .Build();

        var users = await userCollection.GetUsersAsync(filters, cancellation: cancellation);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result<AuthenticationResult>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        if (!await passwordHasher.VerifyPasswordAsync(credentials.Password, user.PasswordHash))
        {
            return Result<AuthenticationResult>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        var accessTokenOutcome = await tokenService.GenerateAccessTokenAsync(user, cancellation);
        if (!accessTokenOutcome.IsSuccess)
        {
            return Result<AuthenticationResult>.Failure(accessTokenOutcome.Error);
        }

        var refreshTokenOutcome = await tokenService.GenerateRefreshTokenAsync(user, cancellation);
        if (!refreshTokenOutcome.IsSuccess)
        {
            return Result<AuthenticationResult>.Failure(refreshTokenOutcome.Error);
        }

        if (accessTokenOutcome.Data is null || refreshTokenOutcome.Data is null)
        {
            return Result<AuthenticationResult>.Failure(AuthenticationErrors.InvalidTokenFormat);
        }

        var result = new AuthenticationResult
        {
            AccessToken = accessTokenOutcome.Data.Value,
            RefreshToken = refreshTokenOutcome.Data.Value
        };

        return Result<AuthenticationResult>.Success(result);
    }
}