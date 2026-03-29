namespace HttpsRichardy.Federation.WebApi.Extensions;

public static class OpenApiExtension
{
    public static void AddOpenApiSpecification(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var host = provider.GetRequiredService<IHostInformationProvider>();

        services.AddOpenApi(options =>
        {
            options.AddScalarTransformers();
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
                document.Components.SecuritySchemes[SecuritySchemes.Bearer] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter 'Bearer' and then your valid token."
                };

                document.Components.SecuritySchemes[Headers.Realm] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    Name = Headers.Realm,
                    In = ParameterLocation.Header,
                    Description = "Realm name used to determine the current realm context for the request"
                };

                document.Components.SecuritySchemes[SecuritySchemes.OAuth2] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri(host.Address.ToString() + "api/v1/protocol/open-id/connect/token")
                        }
                    }
                };

                document.SecurityRequirements ??= [];
                document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                {
                    [document.Components.SecuritySchemes[SecuritySchemes.Bearer]] = Array.Empty<string>(),
                    [document.Components.SecuritySchemes[Headers.Realm]] = Array.Empty<string>()
                });

                document.Info.Contact = new OpenApiContact
                {
                    Name = "Richard Garcia",
                    Email = "code.richardy@gmail.com",
                    Url = new Uri("https://github.com/https-richardy/httpsrichardy.federation.webapi")
                };

                document.Info.License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://github.com/https-richardy/httpsrichardy.federation.webapi/blob/master/LICENSE")
                };

                return Task.CompletedTask;
            });
        });
    }
}
