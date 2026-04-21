namespace HttpsRichardy.Federation.Domain.Collections;

public interface IClientCollection : IAggregateCollection<Client>
{
    public Task<IReadOnlyCollection<Client>> GetClientsAsync(
        ClientFilters filters,
        CancellationToken cancellation = default
    );

    public Task<System.Numerics.BigInteger> CountClientsAsync(
        ClientFilters filters,
        CancellationToken cancellation = default
    );
}