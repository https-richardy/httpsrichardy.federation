namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IRealmsClient
{
    public Task<Result<Pagination<RealmDetails>>> GetRealmsAsync(
        RealmFetchParameters parameters,
        CancellationToken cancellation = default
    );

    public Task<Result<RealmDetails>> CreateRealmAsync(
        RealmCreationScheme realm,
        CancellationToken cancellation = default
    );

    public Task<Result<RealmDetails>> UpdateRealmAsync(
        RealmUpdateScheme realm,
        CancellationToken cancellation = default
    );

    public Task<Result> DeleteRealmAsync(
        string realmId,
        CancellationToken cancellation = default
    );
}
