namespace HttpsRichardy.Federation.Sdk.Contracts.Filtering;

public sealed class GroupFilters
{
    internal string? Id { get; private set; }
    internal string? RealmId { get; private set; }
    internal string? Name { get; private set; }

    internal bool? IsDeleted { get; private set; }

    internal DateOnly? CreatedAfter { get; private set; }
    internal DateOnly? CreatedBefore { get; private set; }

    public GroupFilters WithIdentifier(string identifier)
    {
        if (!string.IsNullOrWhiteSpace(identifier))
        {
            Id = identifier.Trim();
        }

        return this;
    }

    public GroupFilters WithRealmId(string realmId)
    {
        if (!string.IsNullOrWhiteSpace(realmId))
        {
            RealmId = realmId.Trim();
        }

        return this;
    }

    public GroupFilters WithName(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        return this;
    }

    public GroupFilters WithIsDeleted(bool? isDeleted)
    {
        IsDeleted = isDeleted;
        return this;
    }

    public GroupFilters WithCreatedAfter(DateOnly? createdAfter)
    {
        CreatedAfter = createdAfter;
        return this;
    }

    public GroupFilters WithCreatedBefore(DateOnly? createdBefore)
    {
        CreatedBefore = createdBefore;
        return this;
    }

    public static GroupsFetchParameters WithoutFilters => new();
    public static GroupFilters AsBuilder() => new();

    public GroupsFetchParameters Build()
    {
        return new GroupsFetchParameters
        {
            Id = Id,
            RealmId = RealmId,
            Name = Name,
            IsDeleted = IsDeleted,
            CreatedAfter = CreatedAfter,
            CreatedBefore = CreatedBefore
        };
    }
}
