namespace HttpsRichardy.Federation.WebApi.Middlewares;

[ExcludeFromCodeCoverage]
public static class RealmMiddlewareExtensions
{
    public static IApplicationBuilder UseRealmMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RealmMiddleware>();
    }
}
