namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(PermissionsConventions))]
[RealmRequired]
[Route("api/v1/permissions")]
public sealed class PermissionsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetPermissionsAsync([FromQuery] PermissionsFetchParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.
        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),
        };
    }

    [HttpPost]
    [Authorize(Roles = Permissions.CreatePermission)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> CreatePermissionAsync([FromBody] PermissionCreationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionNameIsReserved =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Permissions.EditPermission)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> UpdatePermissionAsync([FromRoute] string id, [FromBody] PermissionUpdateScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { PermissionId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionNameIsReserved =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Permissions.DeletePermission)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> DeletePermissionAsync([FromRoute] string id, [FromQuery] PermissionDeletionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { PermissionId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }
}
