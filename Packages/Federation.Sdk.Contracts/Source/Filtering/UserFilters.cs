namespace HttpsRichardy.Federation.Sdk.Contracts.Filtering;

public sealed class UserFilters
{
    internal string? Id { get; private set; }
    internal string? Username { get; private set; }

    internal bool? IsDeleted { get; private set; }

    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 60;

    public UserFilters WithIdentifier(string identifier)
    {
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            Id = identifier.Trim();
        }

        return this;
    }

    public UserFilters WithUsername(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            Username = username.Trim();
        }

        return this;
    }

    public UserFilters WithIsDeleted(bool? isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }

    public UserFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public UserFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static UsersFetchParameters WithoutFilters => new();
    public static UserFilters AsBuilder() => new();

    public UsersFetchParameters Build()
    {
        return new UsersFetchParameters
        {
            Id = Id,
            Username = Username,
            IsDeleted = IsDeleted,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
