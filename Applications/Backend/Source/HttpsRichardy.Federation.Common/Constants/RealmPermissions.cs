namespace HttpsRichardy.Federation.Common.Constants;

public static class RealmPermissions
{
    public static readonly HashSet<string> InitialPermissions =
    [
        Permissions.CreateGroup,
        Permissions.DeleteGroup,
        Permissions.ViewGroups,
        Permissions.EditGroup,

        Permissions.DeleteUser,
        Permissions.EditUser,
        Permissions.ViewUsers,

        Permissions.CreatePermission,
        Permissions.AssignPermissions,
        Permissions.RevokePermissions,
        Permissions.ViewPermissions,
        Permissions.EditPermission,
        Permissions.DeletePermission,
    ];

    public static readonly HashSet<string> SystemPermissions =
    [
        Permissions.CreateGroup,
        Permissions.DeleteGroup,
        Permissions.EditGroup,
        Permissions.ViewGroups,

        Permissions.DeleteUser,
        Permissions.EditUser,
        Permissions.ViewUsers,

        Permissions.CreatePermission,
        Permissions.AssignPermissions,
        Permissions.RevokePermissions,
        Permissions.ViewPermissions,
        Permissions.EditPermission,
        Permissions.DeletePermission,

        Permissions.CreateRealm,
        Permissions.DeleteRealm,
        Permissions.EditRealm,
        Permissions.ViewRealms,

        Permissions.CreateClient,
        Permissions.DeleteClient,
        Permissions.EditClient,
        Permissions.ViewClients
    ];
}
