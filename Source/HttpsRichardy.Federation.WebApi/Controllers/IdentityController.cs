namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(IdentityConventions))]
[Route("api/v1/identity")]
public sealed class IdentityController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("principal")]
    [Authorize]
    [RealmRequired]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetPrincipalAsync([FromQuery] InspectPrincipalParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost]
    [RealmRequired]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> EnrollIdentityAsync(IdentityEnrollmentCredentials request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == IdentityErrors.UserAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpPost("authenticate")]
    [RealmRequired]
    [Stability(Stability.Deprecated)]
    public async Task<IActionResult> AuthenticateAsync(AuthenticationCredentials request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidCredentials =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.UserNotFound =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("refresh-token")]
    [RealmRequired]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> RefreshTokenAsync(SessionTokenRenewalScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidRefreshToken =>
                StatusCode(StatusCodes.Status400BadRequest, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.TokenExpired =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidSignature =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidTokenFormat =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.UserNotFound =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("invalidate-session")]
    [RealmRequired]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> InvalidateSessionAsync(SessionInvalidationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidRefreshToken =>
                StatusCode(StatusCodes.Status400BadRequest, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.TokenExpired =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidSignature =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.LogoutFailed =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidTokenFormat =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),
        };
    }
}
