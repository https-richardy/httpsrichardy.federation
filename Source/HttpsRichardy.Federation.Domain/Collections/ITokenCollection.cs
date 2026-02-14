namespace HttpsRichardy.Federation.Domain.Collections;

public interface ITokenCollection : IAggregateCollection<SecurityToken>
{
    public Task<IReadOnlyCollection<SecurityToken>> GetTokensAsync(
        TokenFilters filters,
        CancellationToken cancellation = default
    );

    public Task<long> CountAsync(
        TokenFilters filters,
        CancellationToken cancellation = default
    );
}