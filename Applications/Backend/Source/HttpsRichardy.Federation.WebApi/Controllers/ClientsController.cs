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

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == ClientErrors.ClientAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpPut("{id}")]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.EditClient)]
    public async Task<IActionResult> UpdateClientAsync([FromRoute] string id, [FromBody] ClientUpdateScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Id = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == ClientErrors.ClientDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == ClientErrors.ClientAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}")]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.DeleteClient)]
    public async Task<IActionResult> DeleteClientAsync([FromRoute] string id, [FromQuery] ClientDeletionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Id = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == ClientErrors.ClientDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpGet("{id}/permissions")]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.ViewPermissions)]
    public async Task<IActionResult> GetClientPermissionsAsync([FromRoute] string id, [FromQuery] ListClientAssignedPermissionsParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Id = id }, cancellation);

        return result switch
        {

            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == ClientErrors.ClientDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("{id}/permissions")]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.AssignPermissions)]
    public async Task<IActionResult> AssignPermissionAsync([FromRoute] string id, [FromBody] AssignClientPermissionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { Id = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == ClientErrors.ClientDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == ClientErrors.ClientAlreadyHasPermission =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    [Stability(Stability.Stable)]
    [Authorize(Roles = Permissions.RevokePermissions)]
    public async Task<IActionResult> RevokePermissionAsync([FromRoute] string id, [FromRoute] string permissionId, CancellationToken cancellation)
    {
        var request = new RevokeClientPermissionScheme { Id = id, PermissionId = permissionId };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == ClientErrors.ClientDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == ClientErrors.PermissionNotAssigned =>
                StatusCode(StatusCodes.Status409Conflict, result.Error)
        };
    }
}
