namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class SessionTokenRenewalHandler(IUserCollection userCollection, ISecurityTokenService tokenService) :
    IDispatchHandler<SessionTokenRenewalScheme, Result<AuthenticationResult>>
{
    public async Task<Result<AuthenticationResult>> HandleAsync(SessionTokenRenewalScheme parameters, CancellationToken cancellation = default)
    {
        var refreshToken = TokenMapper.AsRefreshToken(parameters.RefreshToken);
        var validationResult = await tokenService.ValidateRefreshTokenAsync(refreshToken, cancellation);

        if (validationResult.IsFailure)
        {
            return Result<AuthenticationResult>.Failure(validationResult.Error);
        }

        var revokeResult = await tokenService.RevokeRefreshTokenAsync(refreshToken, cancellation);
        if (revokeResult.IsFailure)
        {
            return Result<AuthenticationResult>.Failure(revokeResult.Error);
        }

        var userFilters = UserFilters.WithSpecifications()
            .WithSecurityToken(refreshToken.Value)
            .Build();

        var users = await userCollection.GetUsersAsync(userFilters, cancellation: cancellation);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result<AuthenticationResult>.Failure(AuthenticationErrors.UserNotFound);
        }

        var accessTokenResult = await tokenService.GenerateAccessTokenAsync(user, cancellation);
        if (accessTokenResult.IsFailure)
        {
            return Result<AuthenticationResult>.Failure(accessTokenResult.Error);
        }

        var refreshTokenResult = await tokenService.GenerateRefreshTokenAsync(user, cancellation);
        if (refreshTokenResult.IsFailure)
        {
            return Result<AuthenticationResult>.Failure(refreshTokenResult.Error);
        }

        var renewedAccessToken = accessTokenResult.Data!.Value;
        var renewedRefreshToken = refreshTokenResult.Data!.Value;

        var response = new AuthenticationResult
        {
            AccessToken = renewedAccessToken,
            RefreshToken = renewedRefreshToken
        };

        return Result<AuthenticationResult>.Success(response);
    }
}
