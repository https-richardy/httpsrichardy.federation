namespace HttpsRichardy.Federation.Sdk.Contracts.Filtering;

public sealed class GroupPermissionsFilters
{
    internal string? PermissionName { get; private set; }

    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 60;

    public GroupPermissionsFilters WithPermissionName(string permissionName)
    {
        if (!string.IsNullOrWhiteSpace(permissionName))
        {
            PermissionName = permissionName.Trim();
        }

        return this;
    }

    public GroupPermissionsFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public GroupPermissionsFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static ListGroupPermissionsParameters WithoutFilters => new();
    public static GroupPermissionsFilters AsBuilder() => new();

    public ListGroupPermissionsParameters Build()
    {
        return new ListGroupPermissionsParameters
        {
            PermissionName = PermissionName,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
