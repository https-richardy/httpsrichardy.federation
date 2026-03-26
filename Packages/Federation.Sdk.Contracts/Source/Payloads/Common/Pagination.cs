namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Common;

public sealed record Pagination<TItem>
{
    public IReadOnlyCollection<TItem> Items { get; init; } = [];

    public int Total { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}