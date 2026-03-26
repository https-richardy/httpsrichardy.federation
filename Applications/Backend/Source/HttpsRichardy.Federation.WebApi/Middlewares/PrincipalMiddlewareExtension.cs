namespace HttpsRichardy.Federation.WebApi.Middlewares;

[ExcludeFromCodeCoverage(Justification = "")]
public static class PrincipalMiddlewareExtensions
{
    public static IApplicationBuilder UsePrincipalMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<PrincipalMiddleware>();
    }
}
