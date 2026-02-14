namespace HttpsRichardy.Federation.Application.Payloads.Scope;

public sealed record ScopeCreationScheme : IDispatchable<Result<ScopeDetailsScheme>>
{
    public string Name { get; init; } = default!;
    public string Description { get; init; } = default!;

    [JsonIgnore]
    public bool IsGlobal { get; init; }
}