namespace HttpsRichardy.Federation.WebApi.Controllers;

[ApiController]
[RealmRequired]
[Route("api/v1/groups")]
public sealed class GroupsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Permissions.ViewGroups)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetGroupsAsync(
        [FromQuery] GroupsFetchParameters request, CancellationToken cancellation)
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
    [Authorize(Roles = Permissions.CreateGroup)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> CreateGroupAsync(GroupCreationScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status201Created, result.Data),

            { IsFailure: true } when result.Error == GroupErrors.GroupAlreadyExists =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Permissions.EditGroup)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> UpdateGroupAsync(string id, GroupUpdateScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { GroupId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Permissions.DeleteGroup)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> DeleteGroupAsync(string id, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(new GroupDeletionScheme { GroupId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpGet("{id}/permissions")]
    [Authorize(Roles = Permissions.ViewPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> GetGroupsPermissionsAsync(
        [FromRoute] string id,
        [FromQuery] ListGroupAssignedPermissionsParameters request, CancellationToken cancellation
    )
    {
        var result = await dispatcher.DispatchAsync(request with { GroupId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } => StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),
        };
    }

    [HttpPost("{id}/permissions")]
    [Authorize(Roles = Permissions.AssignPermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> AssignPermissionAsync(
        string id, AssignGroupPermissionScheme request, CancellationToken cancellation)
    {
        var result = await dispatcher.DispatchAsync(request with { GroupId = id }, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status200OK, result.Data),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == GroupErrors.GroupAlreadyHasPermission =>
                StatusCode(StatusCodes.Status409Conflict, result.Error),
        };
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    [Authorize(Roles = Permissions.RevokePermissions)]
    [Stability(Stability.Stable)]
    public async Task<IActionResult> RevokePermissionAsync(string id, string permissionId, CancellationToken cancellation)
    {
        var request = new RevokeGroupPermissionScheme { GroupId = id, PermissionId = permissionId };
        var result = await dispatcher.DispatchAsync(request, cancellation);

        return result switch
        {
            { IsSuccess: true } =>
                StatusCode(StatusCodes.Status204NoContent),

            { IsFailure: true } when result.Error == GroupErrors.GroupDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == PermissionErrors.PermissionDoesNotExist =>
                StatusCode(StatusCodes.Status404NotFound, result.Error),

            { IsFailure: true } when result.Error == GroupErrors.PermissionNotAssigned =>
                StatusCode(StatusCodes.Status409Conflict, result.Error)
        };
    }
}
