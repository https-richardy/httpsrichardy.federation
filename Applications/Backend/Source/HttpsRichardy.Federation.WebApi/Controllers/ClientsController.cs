namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[RealmRequired]
[Route("api/v1/clients")]
public sealed class ClientsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.CreateClient)]
    public async Task<IActionResult> CreateClientAsync([FromBody] ClientCreationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.
        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status201Created)
        };
    }
}
