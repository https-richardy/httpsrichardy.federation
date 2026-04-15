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
}
