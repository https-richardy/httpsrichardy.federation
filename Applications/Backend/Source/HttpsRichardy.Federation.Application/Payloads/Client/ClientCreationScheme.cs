namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ClientCreationScheme : IDispatchable<Result>
{
    public string Name { get; init; } = default!;

    public IEnumerable<Grant> Flows { get; init; } = [];
    public IEnumerable<String> RedirectUris { get; init; } = [];
}