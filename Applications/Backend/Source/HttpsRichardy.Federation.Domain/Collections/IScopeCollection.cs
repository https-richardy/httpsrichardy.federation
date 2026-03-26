namespace HttpsRichardy.Federation.Domain.Collections;

public interface IScopeCollection : IAggregateCollection<Scope>
{
    public Task<IReadOnlyCollection<Scope>> GetScopesAsync(
        ScopeFilters filters,
        CancellationToken cancellation = default
    );

    public Task<long> CountAsync(
        ScopeFilters filters,
        CancellationToken cancellation = default
    );
}