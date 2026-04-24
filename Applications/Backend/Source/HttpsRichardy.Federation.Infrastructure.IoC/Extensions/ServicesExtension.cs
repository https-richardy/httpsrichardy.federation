namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class ServicesExtension
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = services.ConfigureSettings(configuration);

        services.AddDataPersistence(settings);
        services.AddServices();
        services.AddMediator();
        services.AddValidators();
    }
}
