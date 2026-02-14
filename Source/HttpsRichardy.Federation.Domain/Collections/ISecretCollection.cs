namespace HttpsRichardy.Federation.Domain.Collections;

public interface ISecretCollection : IAggregateCollection<Secret>
{
    public Task<Secret> GetSecretAsync(CancellationToken cancellation = default);
}
