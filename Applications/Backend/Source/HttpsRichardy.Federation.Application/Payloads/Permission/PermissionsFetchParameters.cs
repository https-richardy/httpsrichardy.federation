namespace HttpsRichardy.Federation.Application.Payloads.Permission;

public sealed record PermissionsFetchParameters :
    IDispatchable<Result<Pagination<PermissionDetailsScheme>>>
{
    public string? Id { get; init; }
    public string? Name { get; set; }
    public bool? IsDeleted { get; set; }

    public PaginationFilters? Pagination { get; set; }
    public SortFilters? Sort { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}