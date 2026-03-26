namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record GroupCreationScheme
{
    public string Name { get; init; } = default!;
}