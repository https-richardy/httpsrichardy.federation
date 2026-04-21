namespace HttpsRichardy.Federation.Application.Services;

public interface ISecretRotationService
{
    public Task EnsureSecretExistsAsync(
        Realm realm,
        CancellationToken cancellation = default
    );

    public Task CreateSecretAsync(
        Realm realm,
        CancellationToken cancellation = default
    );

    public Task RotateSecretAsync(
        Realm realm,
        CancellationToken cancellation = default
    );

    public Task DeleteSecretAsync(
       Realm realm,
       Secret secret,
       CancellationToken cancellation = default
   );
}
