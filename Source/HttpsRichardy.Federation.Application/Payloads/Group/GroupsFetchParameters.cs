namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupsFetchParameters :
    IDispatchable<Result<Pagination<GroupDetailsScheme>>>
{
    public string? Id { get; set; }
    public string? RealmId { get; set; }
    public string? Name { get; set; }
    public bool? IsDeleted { get; set; }

    public PaginationFilters? Pagination { get; set; }
    public SortFilters? Sort { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}