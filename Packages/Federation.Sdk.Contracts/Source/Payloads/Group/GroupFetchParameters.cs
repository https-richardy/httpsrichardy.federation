namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record GroupsFetchParameters
{
    public string? Id { get; set; }
    public string? RealmId { get; set; }
    public string? Name { get; set; }
    public bool? IsDeleted { get; set; }

    public DateOnly? CreatedAfter { get; set; }
    public DateOnly? CreatedBefore { get; set; }
}