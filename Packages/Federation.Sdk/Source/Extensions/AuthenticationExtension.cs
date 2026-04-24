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
                configuration.MetadataAddress = $"{options.Authority}/{options.Realm}/.well-known/openid-configuration";
                configuration.RequireHttpsMetadata = false;
                configuration.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,

                    // https://www.rfc-editor.org/rfc/rfc7519.html?#section-4.1.3
                    // supports multiple audiences in the "aud" claim, so we need to check if any of the audiences match
                    ValidIssuer = options.Authority,
                    ValidAudiences = options.Audiences
                };
            });
    }
}
