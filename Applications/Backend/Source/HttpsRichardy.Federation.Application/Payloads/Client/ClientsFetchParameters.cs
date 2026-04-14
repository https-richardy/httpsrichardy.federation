namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ClientsFetchParameters :
    IDispatchable<Result<Pagination<ClientScheme>>>
{
    public string? Name { get; set; }
    public string? ClientId { get; set; }

    public PaginationFilters? Pagination { get; set; }
    public SortFilters? Sort { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}
