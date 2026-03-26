namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record GroupBasicDetails
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}
