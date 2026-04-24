namespace HttpsRichardy.Federation.Sdk.Contracts.Filtering;

public sealed class UserGroupsFilters
{
    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 60;

    public UserGroupsFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public UserGroupsFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static ListUserAssignedGroupsParameters WithoutFilters => new();
    public static UserGroupsFilters AsBuilder() => new();

    public ListUserAssignedGroupsParameters Build()
    {
        return new ListUserAssignedGroupsParameters
        {
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
