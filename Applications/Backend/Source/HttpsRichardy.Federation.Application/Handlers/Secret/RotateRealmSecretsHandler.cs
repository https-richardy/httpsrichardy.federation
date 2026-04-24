namespace HttpsRichardy.Federation.Application.Handlers.Secret;

public sealed class RotateRealmSecretsHandler(ISecretRotationService rotationService, IRealmCollection realmCollection) :
    IDispatchHandler<RotateRealmSecretsParameters, Result>
{
    public async Task<Result> HandleAsync(
        RotateRealmSecretsParameters parameters, CancellationToken cancellation = default)
    {
        var realmFilters = RealmFilters.WithSpecifications()
            .WithIdentifier(parameters.RealmId)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result.Failure(RealmErrors.RealmDoesNotExist);
        }

        await rotationService.RotateSecretAsync(realm, cancellation);

        return Result.Success();
    }
}
