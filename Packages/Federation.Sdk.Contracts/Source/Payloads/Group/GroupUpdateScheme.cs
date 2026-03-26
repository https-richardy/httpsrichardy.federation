namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record GroupUpdateScheme
{
    [JsonIgnore]
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}