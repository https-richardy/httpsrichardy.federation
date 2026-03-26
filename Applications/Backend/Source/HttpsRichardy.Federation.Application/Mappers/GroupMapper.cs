namespace HttpsRichardy.Federation.Application.Mappers;

public static class GroupMapper
{
    public static Group AsGroup(GroupCreationScheme group, Realm realm) => new()
    {
        Name = group.Name,
        RealmId = realm.Id
    };

    public static Group AsGroup(GroupUpdateScheme payload, Group group)
    {
        group.Name = payload.Name;

        return group;
    }

    public static GroupDetailsScheme AsResponse(Group group) => new()
    {
        Id = group.Id.ToString(),
        Name = group.Name,
        Permissions = [.. group.Permissions.Select(group => PermissionMapper.AsResponse(group))]
    };

    public static IReadOnlyCollection<GroupBasicDetailsScheme> AsBasicResponse(IEnumerable<Group> groups)
    {
        return [.. groups.Select(GroupMapper.AsBasicResponse)];
    }

    public static GroupBasicDetailsScheme AsBasicResponse(Group group) => new()
    {
        Id = group.Id.ToString(),
        Name = group.Name,
    };

    public static GroupFilters AsFilters(GroupsFetchParameters parameters) => new()
    {
        Id = parameters.Id,
        RealmId = parameters.RealmId,
        Name = parameters.Name,
        Pagination = parameters.Pagination,
        Sort = parameters.Sort,
        IsDeleted = parameters.IsDeleted
    };
}