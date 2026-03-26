#pragma warning disable IDE0060

namespace HttpsRichardy.Federation.WebApi.Conventions;

public static class WellKnownConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(OpenIDConfigurationScheme), StatusCodes.Status200OK)]
    public static void GetConfigurationAsync(FetchOpenIDConfigurationParameters request, CancellationToken cancellation) { }

    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(JsonWebKeySetScheme), StatusCodes.Status200OK)]
    public static void GetJsonWebKeysAsync(FetchJsonWebKeysParameters request, CancellationToken cancellation) { }
}
