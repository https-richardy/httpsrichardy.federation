namespace HttpsRichardy.Federation.WebApi.Workers;

public sealed class KeyRotationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<KeyRotationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan _rotationInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = scopeFactory.CreateScope();

            var rotationService = scope.ServiceProvider.GetRequiredService<ISecretRotationService>();
            var realmCollection = scope.ServiceProvider.GetRequiredService<IRealmCollection>();
            var secretCollection = scope.ServiceProvider.GetRequiredService<ISecretCollection>();

            var realms = await realmCollection.GetRealmsAsync(RealmFilters.WithoutFilters, stoppingToken);

            await Parallel.ForEachAsync(realms, stoppingToken, async (realm, cancellation) =>
            {
                try
                {
                    logger.LogInformation("rotating keys for realm {realm}", realm.Name);

                    await rotationService.EnsureSecretExistsAsync(realm, cancellation);

                    var now = DateTime.UtcNow;
                    var filters = SecretFilters.WithSpecifications()
                        .WithRealm(realm.Id)
                        .WithCanSign(now)
                        .Build();

                    var secrets = await secretCollection.GetSecretsAsync(filters, cancellation);
                    var current = secrets
                        .OrderByDescending(secret => secret.CreatedAt)
                        .FirstOrDefault();

                    if (current is not null && now - current.CreatedAt >= _rotationInterval)
                    {
                        await rotationService.RotateSecretAsync(realm, cancellation);
                    }

                    await rotationService.PruneSecretsAsync(realm, cancellation);
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "an error occurred while rotating keys for realm {realm}", realm.Name);
                }
            });

            await Task.Delay(_rotationInterval, stoppingToken);
        }
    }
}
