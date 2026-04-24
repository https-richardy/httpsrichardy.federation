namespace HttpsRichardy.Federation.Sdk.Filtering;

public sealed class PermissionFilters
{
    internal string? Name { get; private set; }
    internal bool IncludeDeleted { get; private set; } = false;

    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 20;

    public PermissionFilters WithName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        return this;
    }

    public PermissionFilters WithIncludeDeleted(bool includeDeleted)
    {
        IncludeDeleted = includeDeleted;
        return this;
    }

    public PermissionFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public PermissionFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static PermissionsFetchParameters WithoutFilters => new();
    public static PermissionFilters AsBuilder() => new();

    public PermissionsFetchParameters Build()
    {
        return new PermissionsFetchParameters
        {
            Name = Name,
            IncludeDeleted = IncludeDeleted,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
