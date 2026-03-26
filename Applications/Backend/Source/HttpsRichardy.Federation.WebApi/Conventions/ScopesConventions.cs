#pragma warning disable IDE0060

namespace HttpsRichardy.Federation.WebApi.Conventions;

public static class ScopesConventions
{
    [ApiConventionNameMatch(ApiConventionNameMatchBehavior.Prefix)]
    [ProducesResponseType(typeof(ScopeDetailsScheme), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Error), StatusCodes.Status409Conflict)]
    public static void CreateScopeAsync(ScopeCreationScheme request, CancellationToken cancellation) { }
}
