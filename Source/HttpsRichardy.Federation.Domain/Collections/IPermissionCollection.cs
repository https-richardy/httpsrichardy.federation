namespace HttpsRichardy.Federation.Domain.Collections;

public interface IPermissionCollection : IAggregateCollection<Permission>
{
    public Task<IReadOnlyCollection<Permission>> GetPermissionsAsync(
        PermissionFilters filters,
        CancellationToken cancellation = default
    );

    public Task<long> CountAsync(
        PermissionFilters filters,
        CancellationToken cancellation = default
    );
}