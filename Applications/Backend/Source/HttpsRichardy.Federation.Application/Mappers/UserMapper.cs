namespace HttpsRichardy.Federation.Application.Mappers;

public static class UserMapper
{
    public static User AsUser(IdentityEnrollmentCredentials credentials, string realmId) => new()
    {
        Username = credentials.Username,
        RealmId = realmId,
    };

    public static UserFilters AsFilters(UsersFetchParameters parameters) => new()
    {
        Id = parameters.Id,
        Username = parameters.Username,
        IsDeleted = parameters.IsDeleted,
        Pagination = parameters.Pagination,
        Sort = parameters.Sort
    };

    public static UserDetailsScheme AsResponse(User user) => new()
    {
        Id = user.Id.ToString(),
        Username = user.Username
    };

    public static PrincipalDetailsScheme AsPrincipal(User user) => new()
    {
        Id = user.Id.ToString(),
        Username = user.Username,
        Permissions = [.. user.Permissions.Select(permission => PermissionMapper.AsResponse(permission))],
        Groups = [.. user.Groups.Select(group => GroupMapper.AsBasicResponse(group))],
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}