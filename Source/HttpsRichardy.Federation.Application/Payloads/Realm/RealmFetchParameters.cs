namespace HttpsRichardy.Federation.Application.Payloads.Realm;

public sealed record RealmFetchParameters :
    IDispatchable<Result<Pagination<RealmDetailsScheme>>>
{
    public string? Id { get; init; }
    public string? Name { get; init; }
    public string? ClientId { get; init; }
    public bool? IsDeleted { get; init; }

    public PaginationFilters? Pagination { get; set; }
    public SortFilters? Sort { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}
