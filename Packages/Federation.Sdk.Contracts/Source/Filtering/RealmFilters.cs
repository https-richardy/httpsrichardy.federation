namespace HttpsRichardy.Federation.Sdk.Contracts.Filtering;

public sealed class RealmFilters
{
    internal string? Id { get; private set; }
    internal string? Name { get; private set; }

    internal bool? IncludeDeleted { get; private set; }

    internal int PageNumber { get; private set; } = 1;
    internal int PageSize { get; private set; } = 20;

    public RealmFilters WithIdentifier(string identifier)
    {
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            Id = identifier.Trim();
        }

        return this;
    }

    public RealmFilters WithName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        return this;
    }

    public RealmFilters WithIncludeDeleted(bool? includeDeleted)
    {
        IncludeDeleted = includeDeleted;
        return this;
    }

    public RealmFilters WithPageNumber(int pageNumber)
    {
        if (pageNumber > 0)
        {
            PageNumber = pageNumber;
        }

        return this;
    }

    public RealmFilters WithPageSize(int pageSize)
    {
        if (pageSize > 0)
        {
            PageSize = pageSize;
        }

        return this;
    }

    public static RealmFetchParameters WithoutFilters => new();
    public static RealmFilters AsBuilder() => new();

    public RealmFetchParameters Build()
    {
        return new RealmFetchParameters
        {
            Id = Id,
            Name = Name,
            IncludeDeleted = IncludeDeleted,
            PageNumber = PageNumber,
            PageSize = PageSize
        };
    }
}
