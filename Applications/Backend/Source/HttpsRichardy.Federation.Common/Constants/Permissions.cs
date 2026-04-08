namespace HttpsRichardy.Federation.Common.Constants;

public static class Permissions
{
    public const string CreateGroup = "federation.defaults.permissions.group.create";
    public const string DeleteGroup = "federation.defaults.permissions.group.delete";
    public const string EditGroup = "federation.defaults.permissions.group.update";
    public const string ViewGroups = "federation.defaults.permissions.group.view";

    public const string DeleteUser = "federation.defaults.permissions.user.delete";
    public const string EditUser = "federation.defaults.permissions.user.update";
    public const string ViewUsers = "federation.defaults.permissions.user.view";

    public const string CreatePermission = "federation.defaults.permissions.permissions.create";
    public const string AssignPermissions = "federation.defaults.permissions.permissions.assign";
    public const string RevokePermissions = "federation.defaults.permissions.permissions.revoke";
    public const string ViewPermissions = "federation.defaults.permissions.permissions.view";
    public const string EditPermission = "federation.defaults.permissions.permissions.edit";
    public const string DeletePermission = "federation.defaults.permissions.permissions.delete";

    public const string CreateRealm = "federation.defaults.permissions.realm.create";
    public const string DeleteRealm = "federation.defaults.permissions.realm.delete";
    public const string EditRealm = "federation.defaults.permissions.realm.update";
    public const string ViewRealms = "federation.defaults.permissions.realm.view";

    public const string CreateClient = "federation.defaults.permissions.client.create";
    public const string DeleteClient = "federation.defaults.permissions.client.delete";
    public const string EditClient = "federation.defaults.permissions.client.update";
    public const string ViewClients = "federation.defaults.permissions.client.view";
}
