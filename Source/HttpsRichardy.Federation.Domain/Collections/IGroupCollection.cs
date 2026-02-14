namespace HttpsRichardy.Federation.Domain.Collections;

public interface IGroupCollection : IAggregateCollection<Group>
{
    public Task<IReadOnlyCollection<Group>> GetGroupsAsync(
        GroupFilters filters,
        CancellationToken cancellation = default
    );

    public Task<long> CountAsync(
        GroupFilters filters,
        CancellationToken cancellation = default
    );
}