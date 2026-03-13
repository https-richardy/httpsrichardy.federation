namespace HttpsRichardy.Federation.Domain.Policies;

// defines a policy responsible for protecting the system permission namespace
// from unauthorized usage by realms

// certain permissions are reserved by the federation system and represent
// privileged administrative capabilities (e.g. managing realms or federation resources)

// this policy ensures that realms cannot create or manipulate permissions
// whose identifiers belong to the reserved system namespace, preventing
// privilege escalation through permission name collision

public interface IPermissionNamespacePolicy
{
    public Task<Result> EnsurePermissionIsAllowedAsync(
        Realm realm,
        Permission permission,
        CancellationToken cancellation = default
    );
}
