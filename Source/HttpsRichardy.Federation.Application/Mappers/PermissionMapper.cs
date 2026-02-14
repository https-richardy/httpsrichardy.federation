namespace HttpsRichardy.Federation.Application.Mappers;

public static class PermissionMapper
{
    public static PermissionDetailsScheme AsResponse(Permission permission) => new()
    {
        Id = permission.Id.ToString(),
        Name = permission.Name,
        Description = permission.Description
    };

    public static IReadOnlyCollection<PermissionDetailsScheme> AsResponse(IEnumerable<Permission> permissions)
    {
        return [.. permissions.Select(PermissionMapper.AsResponse)];
    }

    public static PermissionFilters AsFilters(PermissionsFetchParameters parameters) => new()
    {
        Id = parameters.Id,
        Name = parameters.Name,
        Pagination = parameters.Pagination,
        Sort = parameters.Sort,
        IsDeleted = parameters.IsDeleted
    };

    public static Permission AsPermission(PermissionCreationScheme permission, Realm realm) => new()
    {
        Name = permission.Name,
        Description = permission.Description,
        RealmId = realm.Id
    };

    public static Permission AsPermission(PermissionUpdateScheme payload, Permission permission, Realm realm)
    {
        permission.Name = payload.Name;
        permission.Description = payload.Description ?? permission.Description;
        permission.RealmId = realm.Id;

        permission.MarkAsUpdated();

        return permission;
    }
}