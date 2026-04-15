#pragma warning disable IDE0060

namespace HttpsRichardy.Federation.WebApi.Conventions;

public static class ClientsConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(Pagination<ClientScheme>), StatusCodes.Status200OK)]
    public static void GetClientsAsync(ClientsFetchParameters request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public static void CreateClientAsync(ClientCreationScheme request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(IReadOnlyCollection<PermissionDetailsScheme>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status404NotFound)]
    public static void GetClientPermissionsAsync(string id, ListClientAssignedPermissionsParameters request, CancellationToken cancellation) { }
}
