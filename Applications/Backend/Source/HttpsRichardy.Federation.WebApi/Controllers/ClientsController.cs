namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(ClientsConventions))]
[RealmRequired]
[Route("api/v1/clients")]
public sealed class ClientsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.ViewClients)]
    public async Task<IActionResult> GetClientsAsync([FromQuery] ClientsFetchParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data)
        };
    }

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
