#pragma warning disable IDE0060

namespace HttpsRichardy.Federation.WebApi.Conventions;

public static class ConnectConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(ClientAuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status401Unauthorized)]
    public static void AuthenticateClientAsync(ClientAuthenticationCredentials request, CancellationToken cancellation) { }
}
