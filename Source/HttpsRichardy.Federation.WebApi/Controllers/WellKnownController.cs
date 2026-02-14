namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(WellKnownConventions))]
[Route(".well-known")]
public sealed class WellKnownController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("openid-configuration")]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetConfigurationAsync(
        [FromQuery] FetchOpenIDConfigurationParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.
        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),
        };
    }

    [HttpGet("jwks.json")]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetJsonWebKeysAsync(
        [FromQuery] FetchJsonWebKeysParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.
        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),
        };
    }
}
