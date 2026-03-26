namespace HttpsRichardy.Federation.WebApi.Middlewares;

public sealed class PrincipalMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var principalProvider = context.RequestServices.GetRequiredService<IPrincipalProvider>();

        principalProvider.Clear();

        var endpoint = context.GetEndpoint();
        var requiresAuth = endpoint?.Metadata.GetMetadata<AuthorizeAttribute>() != null;

        if (!requiresAuth || context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        var preferredUsernameClaim = context.User.Claims.FirstOrDefault(claim => claim.Type == "preferred_username");

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            await next(context);
            return;
        }

        if (preferredUsernameClaim == null || string.IsNullOrWhiteSpace(preferredUsernameClaim.Value))
        {
            await next(context);
            return;
        }

        var principal = new User()
        {
            Id = userIdClaim.Value,
            Username = preferredUsernameClaim.Value
        };

        principalProvider.SetPrincipal(principal);

        await next(context);
    }
}
