namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[Route("api/v1/protocol/open-id/connect")]
public sealed class ConnectController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost("token")]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> AuthenticateClientAsync(
        [FromSnakeCaseForm] ClientAuthenticationCredentials request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == AuthenticationErrors.ClientNotFound =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthenticationErrors.InvalidClientCredentials =>
                StatusCode(StatusCodes.Status401Unauthorized, result.Error),

            { IsFailure: true } when result.Error == AuthorizationErrors.InvalidAuthorizationCode =>
                StatusCode(StatusCodes.Status400BadRequest, result.Error),

            { IsFailure: true } when result.Error == AuthorizationErrors.InvalidCodeVerifier =>
                StatusCode(StatusCodes.Status400BadRequest, result.Error)
        };
    }
}
