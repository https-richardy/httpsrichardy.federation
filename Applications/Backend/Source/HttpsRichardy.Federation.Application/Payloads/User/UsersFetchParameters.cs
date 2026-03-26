namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record UsersFetchParameters :
    IDispatchable<Result<Pagination<UserDetailsScheme>>>
{
    public string? Id { get; init; }
    public string? Username { get; init; }
    public bool? IsDeleted { get; set; }

    public PaginationFilters? Pagination { get; set; }
    public SortFilters? Sort { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}