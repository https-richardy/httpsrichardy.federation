#pragma warning disable IDE0060

namespace HttpsRichardy.Federation.WebApi.Conventions;

public static class ClientsConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(Pagination<ClientScheme>), StatusCodes.Status200OK)]
    public static void GetClientsAsync(ClientsFetchParameters request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public static void CreateClientAsync(ClientCreationScheme request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(ClientScheme), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public static void UpdateClientAsync(string id, ClientUpdateScheme request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public static void DeleteClientAsync(string id, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PermissionDetailsScheme>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public static void GetClientPermissionsAsync(string id, ListClientAssignedPermissionsParameters request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PermissionDetailsScheme>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public static void AssignPermissionAsync(string id, AssignClientPermissionScheme request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public static void RevokePermissionAsync(string id, string permissionId, CancellationToken cancellation) { }
}
