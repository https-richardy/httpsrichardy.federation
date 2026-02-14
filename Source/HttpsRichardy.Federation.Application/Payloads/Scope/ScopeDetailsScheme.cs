namespace HttpsRichardy.Federation.Application.Payloads.Scope;

public sealed record ScopeDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;
}