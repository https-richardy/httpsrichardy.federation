namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
internal static class CorsConfigurationExtension
{
    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin();
                policy.AllowAnyMethod();
                policy.AllowAnyHeader();
            });
        });
    }
}