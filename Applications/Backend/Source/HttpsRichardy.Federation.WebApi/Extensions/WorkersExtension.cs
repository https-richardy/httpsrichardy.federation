namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class WorkersExtension
{
    public static void AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<KeyRotationBackgroundService>();
    }
}
