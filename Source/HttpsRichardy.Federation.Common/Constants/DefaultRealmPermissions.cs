namespace HttpsRichardy.Federation.Common.Constants;

public static class DefaultRealmPermissions
{
    public static readonly string[] InitialPermissions =
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

        Permissions.CreateScope,
        Permissions.EditScope,
        Permissions.DeleteGroup,
        Permissions.ViewScopes
    ];
}
