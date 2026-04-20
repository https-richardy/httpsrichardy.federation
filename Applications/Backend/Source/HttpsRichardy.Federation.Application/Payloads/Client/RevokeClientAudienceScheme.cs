namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record RevokeClientAudienceScheme :
    IDispatchable<Result<IReadOnlyCollection<string>>>
{
    [JsonIgnore]
    public string Id { get; init; } = default!;

    [JsonIgnore]
    public string Audience { get; init; } = default!;
}
