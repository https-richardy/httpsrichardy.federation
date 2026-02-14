namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class WebInfrastructureExtension
{
    public static void AddWebComposition(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddControllers();
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddCorsConfiguration();
        services.AddJwtAuthentication();
        services.AddMemoryCache();
        services.AddProviders();
        services.AddOpenApiSpecification();
        services.AddRazorPages();
    }
}
