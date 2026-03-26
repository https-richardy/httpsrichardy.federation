namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record GroupDetails
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;

    public IReadOnlyCollection<PermissionDetails> Permissions { get; init; } = [  ];
}