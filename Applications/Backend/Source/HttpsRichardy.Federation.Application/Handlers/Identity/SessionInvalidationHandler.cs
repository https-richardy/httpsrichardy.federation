namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class SessionInvalidationHandler(ISecurityTokenService tokenService) :
    IDispatchHandler<SessionInvalidationScheme, Result>
{
    public async Task<Result> HandleAsync(SessionInvalidationScheme parameters, CancellationToken cancellation = default)
    {
        var refreshToken = TokenMapper.AsRefreshToken(parameters.RefreshToken);
        var validationResult = await tokenService.ValidateRefreshTokenAsync(refreshToken, cancellation);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        var revokeResult = await tokenService.RevokeRefreshTokenAsync(refreshToken, cancellation);
        if (revokeResult.IsFailure)
        {
            return Result.Failure(revokeResult.Error);
        }

        return Result.Success();
    }
}