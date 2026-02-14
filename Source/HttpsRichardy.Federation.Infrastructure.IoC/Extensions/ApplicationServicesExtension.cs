namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class ApplicationServicesExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
        services.AddTransient<ISecurityTokenService, JwtSecurityTokenService>();
        services.AddTransient<IClientCredentialsGenerator, ClientCredentialsGenerator>();
        services.AddTransient<IRedirectUriPolicy, RedirectUriPolicy>();

        services.AddSingleton<IRealmProvider, RealmProvider>();
        services.AddSingleton<IPrincipalProvider, PrincipalProvider>();
    }
}
