namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ClientUpdateScheme : IDispatchable<Result<ClientScheme>>
{
    [JsonIgnore]
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;

    public IEnumerable<Grant> Flows { get; init; } = [];
    public IEnumerable<String> RedirectUris { get; init; } = [];
}
