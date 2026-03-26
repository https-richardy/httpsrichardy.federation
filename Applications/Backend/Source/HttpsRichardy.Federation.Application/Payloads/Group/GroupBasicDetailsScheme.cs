namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupBasicDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}