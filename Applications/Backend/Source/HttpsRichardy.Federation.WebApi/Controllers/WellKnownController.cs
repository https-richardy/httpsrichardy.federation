namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(WellKnownConventions))]
[Route("{realm}/.well-known")]
public sealed class WellKnownController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("openid-configuration")]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetConfigurationAsync([FromQuery] FetchOpenIDConfigurationParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Realm = (string)RouteData.Values["realm"]! }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error)
        };
    }

    [HttpGet("jwks.json")]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetJsonWebKeysAsync([FromQuery] FetchJsonWebKeysParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Realm = (string)RouteData.Values["realm"]! }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error)
        };
    }
}
