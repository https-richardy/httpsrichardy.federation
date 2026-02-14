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

        var userCollection = context.RequestServices.GetRequiredService<IUserCollection>();
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || string.IsNullOrWhiteSpace(userIdClaim.Value))
        {
            await next(context);
            return;
        }

        var filters = UserFilters.WithSpecifications()
            .WithIdentifier(userIdClaim.Value)
            .Build();

        var users = await userCollection.GetUsersAsync(filters, context.RequestAborted);
        var user = users.FirstOrDefault();

        if (user is not null)
        {
            principalProvider.SetPrincipal(user);
        }

        await next(context);
    }
}
