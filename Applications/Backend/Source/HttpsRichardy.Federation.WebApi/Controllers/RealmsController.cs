namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(RealmsConventions))]
[RealmRequired]
[Route("api/v1/realms")]
public sealed class RealmsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.ViewRealms)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetRealmsAsync([FromQuery] RealmFetchParameters request, CancellationToken cancellation)
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
    [Authorize(Roles = Permissions.CreateRealm)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> CreateRealmAsync([FromBody] RealmCreationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Permissions.EditRealm)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> UpdateRealmAsync([FromRoute] string id, [FromBody] RealmUpdateScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { RealmId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Permissions.DeleteRealm)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> DeleteRealmAsync([FromRoute] string id, [FromQuery] RealmDeletionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { RealmId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpGet("{id}/secrets")]
    [Authorize(Roles = Permissions.ViewRealms)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetRealmSecretsAsync([FromRoute] string id, [FromQuery] FetchRealmSecretsParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { RealmId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpGet("{id}/permissions")]
    [Authorize(Roles = Permissions.ViewPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetRealmPermissionsAsync([FromRoute] string id, [FromQuery] ListRealmAssignedPermissionsParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { RealmId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } when result.Data is not null =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("{id}/permissions")]
    [Authorize(Roles = Permissions.AssignPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> AssignPermissionAsync([FromRoute] string id, [FromBody] AssignRealmPermissionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { RealmId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == RealmErrors.RealmAlreadyHasPermission =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    [Authorize(Roles = Permissions.RevokePermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> RevokePermissionAsync([FromRoute] string id, [FromRoute] string permissionId, CancellationToken cancellation)
    {
        var request = new RevokeRealmPermissionScheme { RealmId = id, PermissionId = permissionId };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == RealmErrors.RealmDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == RealmErrors.PermissionNotAssigned =>
                StatusCode(StatusCodes.Status409Conflict, result.Error)
        };
    }
}
