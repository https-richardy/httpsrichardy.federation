namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record AssignClientAudienceScheme :
    IDispatchable<Result<IReadOnlyCollection<string>>>
{
    [JsonIgnore]
    public string Id { get; init; } = default!;
    public string Value { get; init; } = default!;
}
