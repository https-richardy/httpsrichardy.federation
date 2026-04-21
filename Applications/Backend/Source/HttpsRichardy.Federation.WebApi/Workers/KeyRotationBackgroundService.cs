namespace HttpsRichardy.Federation.WebApi.Workers;

public sealed class KeyRotationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<KeyRotationBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();

            var rotationService = scope.ServiceProvider.GetRequiredService<ISecretRotationService>();
            var realmCollection = scope.ServiceProvider.GetRequiredService<IRealmCollection>();

            var realms = await realmCollection.GetRealmsAsync(RealmFilters.WithoutFilters, stoppingToken);

            await Parallel.ForEachAsync(realms, stoppingToken, async (realm, cancellation) =>
            {
                try
                {
                    logger.LogInformation("rotating keys for realm {realm}", realm.Name);

                    /* !important: ensures the realm has at least one valid signing key before rotation */
                    await rotationService.EnsureSecretExistsAsync(realm, cancellation);

                    await rotationService.RotateSecretAsync(realm, cancellation);
                    await rotationService.PruneSecretsAsync(realm, cancellation);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "an error occurred while rotating keys for realm {realm}", realm.Name);
                }
            });

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
