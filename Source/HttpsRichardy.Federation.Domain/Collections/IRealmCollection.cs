namespace HttpsRichardy.Federation.Domain.Collections;

public interface IRealmCollection : IAggregateCollection<Realm>
{
    public Task<IReadOnlyCollection<Realm>> GetRealmsAsync(
        RealmFilters filters,
        CancellationToken cancellation = default
    );

    public Task<long> CountAsync(
        RealmFilters filters,
        CancellationToken cancellation = default
    );
}
