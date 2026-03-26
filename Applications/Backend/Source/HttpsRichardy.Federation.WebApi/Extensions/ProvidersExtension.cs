namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage(Justification = "contains only service registrations")]
public static class ProvidersExtension
{
    public static void AddProviders(this IServiceCollection services)
    {
        services.AddSingleton<IHostInformationProvider, HostInformationProvider>();
    }
}