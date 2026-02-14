namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class SettingsExtension
{
    public static ISettings ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("Settings")
            .Get<Settings>()!;

        services.AddSingleton<ISettings>(settings);

        return settings;
    }
}