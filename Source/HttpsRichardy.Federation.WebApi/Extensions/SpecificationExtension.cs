namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage(Justification = "contains only dependency injection registration with no business logic.")]
public static class SpecificationsExtension
{
    public static void UseSpecification(this IEndpointRouteBuilder app)
    {
        app.MapScalarApiReference(options =>
        {
            options.DarkMode = false;
            options.HideDarkModeToggle = true;
            options.HideClientButton = true;
            options.HideModels = true;
            options.HideSearch = true;
            options.Servers = [];

            options.WithTitle("Federation | Reference");
            options.ExpandAllTags();
            options.AddPreferredSecuritySchemes(SecuritySchemes.Bearer);
            options.AddClientCredentialsFlow(SecuritySchemes.OAuth2, flow =>
            {
                flow.WithCredentialsLocation(CredentialsLocation.Body);
            });
        });
    }
}
