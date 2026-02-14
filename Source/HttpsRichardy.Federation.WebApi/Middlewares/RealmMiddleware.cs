namespace HttpsRichardy.Federation.WebApi.Middlewares;

public sealed class RealmMiddleware(IMemoryCache cache, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var requiresRealm = endpoint?.Metadata.GetMetadata<RealmRequiredAttribute>() != null;

        if (!requiresRealm)
        {
            await next(context);
            return;
        }

        var realmCollection = context.RequestServices.GetRequiredService<IRealmCollection>();
        var realmProvider = context.RequestServices.GetRequiredService<IRealmProvider>();

        var realmHeaderKey = context.Request.Headers.Keys
            .FirstOrDefault(key => string.Equals(key, "realm", StringComparison.OrdinalIgnoreCase));

        if (realmHeaderKey == null || string.IsNullOrWhiteSpace(context.Request.Headers[realmHeaderKey]))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var error = RealmErrors.RealmHeaderMissing;
            var response = JsonSerializer.Serialize(new
            {
                code = error.Code,
                description = error.Description
            });

            await context.Response.WriteAsync(response);
            return;
        }

        var realmName = context.Request.Headers[realmHeaderKey].ToString();
        var cacheKey = $"realm:{realmName}";

        if (!cache.TryGetValue(cacheKey, out Realm? realm))
        {
            var filters = RealmFilters.WithSpecifications()
                .WithName(realmName)
                .Build();

            var realms = await realmCollection.GetRealmsAsync(filters, context.RequestAborted);

            realm = realms.FirstOrDefault();

            if (realm is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = MediaTypeNames.Application.Json;

                var error = RealmErrors.RealmDoesNotExist;
                var response = JsonSerializer.Serialize(new
                {
                    code = error.Code,
                    description = error.Description
                });

                await context.Response.WriteAsync(response);
                return;
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(10));

            cache.Set(cacheKey, realm, cacheOptions);
        }

        context.Items["RealmName"] = realm?.Name;
        realmProvider.SetRealm(realm!);

        await next(context);
    }
}
