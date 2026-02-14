namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[ApiConventionType(typeof(UsersConventions))]
[RealmRequired]
[Route("api/v1/users")]
public sealed class UsersController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.ViewUsers)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetUsersAsync([FromQuery] UsersFetchParameters request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        // we know the switch here is not strictly necessary since we only handle the success case,
        // but we keep it for consistency with the rest of the codebase and to follow established patterns.
        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Permissions.EditUser)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> DeleteUserAsync(string id, CancellationToken cancellation)
    {
        var request = new UserDeletionScheme { UserId = id };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error)
        };
    }

    [HttpGet("{id}/permissions")]
    [Authorize(Roles = Permissions.ViewPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetUserPermissionsAsync(
        [FromRoute] string id,
        [FromQuery] ListUserAssignedPermissionsParameters request, CancellationToken cancellation
    )
    {
        var result = await dispatcher.DispatchAsync(request with { UserId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpGet("{id}/groups")]
    [Authorize(Roles = Permissions.ViewGroups)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetUserGroupsAsync(
        [FromRoute] string id,
        [FromQuery] ListUserAssignedGroupsParameters request, CancellationToken cancellation
    )
    {
        var result = await dispatcher.DispatchAsync(request with { UserId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("{id}/groups")]
    [Authorize(Roles = Permissions.EditUser)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> AssignUserToGroupAsync(
        string id, AssignUserToGroupScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { UserId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == UserErrors.UserAlreadyInGroup =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("{id}/permissions")]
    [Authorize(Roles = Permissions.AssignPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> AssignUserPermissionAsync(
        string id, AssignUserPermissionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { UserId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == UserErrors.UserAlreadyHasPermission =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    [Authorize(Roles = Permissions.RevokePermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> RevokeUserPermissionAsync(
        string id, string permissionId, CancellationToken cancellation)
    {
        var request = new RevokeUserPermissionScheme { UserId = id, PermissionId = permissionId };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == UserErrors.PermissionNotAssigned =>
                StatusCode(StatusCodes.Status409Conflict, result.Error)
        };
    }

    [HttpDelete("{id}/groups/{groupId}")]
    [Authorize(Roles = Permissions.EditUser)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> RemoveUserFromGroupAsync(string id, string groupId, CancellationToken cancellation)
    {
        var request = new RemoveUserFromGroupScheme { UserId = id, GroupId = groupId };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == UserErrors.UserDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == UserErrors.UserNotInGroup =>
                StatusCode(StatusCodes.Status409Conflict, result.Error)
        };
    }
}
