namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[RealmRequired]
[Route("api/v1/scopes")]
public sealed class ScopesController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Permissions.CreateScope)]
    [Stability(Stability.Experimental)]
    public async Task<IActionResult> CreateScopeAsync(ScopeCreationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == ScopeErrors.ScopeAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }
}
