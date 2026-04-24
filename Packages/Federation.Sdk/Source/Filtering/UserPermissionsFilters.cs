namespace HttpsRichardy.Federation.Sdk.Filtering;

public sealed class UserPermissionsFilters
{
    internal string? PermissionName { get; private set; }

    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 60;

    public UserPermissionsFilters WithPermissionName(string permissionName)
    {
        if (!string.IsNullOrWhiteSpace(permissionName))
        {
            PermissionName = permissionName.Trim();
        }

        return this;
    }

    public UserPermissionsFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public UserPermissionsFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static ListUserAssignedPermissionsParameters WithoutFilters => new();
    public static UserPermissionsFilters AsBuilder() => new();

    public ListUserAssignedPermissionsParameters Build()
    {
        return new ListUserAssignedPermissionsParameters
        {
            PermissionName = PermissionName,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
