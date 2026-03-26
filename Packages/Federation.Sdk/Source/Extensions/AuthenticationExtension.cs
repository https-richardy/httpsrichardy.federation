namespace HttpsRichardy.Federation.Sdk.Extensions;

public static class AuthenticationExtension
{
    public static void AddBearerAuthentication(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<FederationOptions>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configuration =>
            {
                configuration.Authority = options.Authority;
                configuration.Audience = options.Realm;
                configuration.RequireHttpsMetadata = false;
            });
    }
}