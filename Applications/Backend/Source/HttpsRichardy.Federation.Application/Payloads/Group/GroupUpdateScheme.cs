namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupUpdateScheme : IDispatchable<Result<GroupDetailsScheme>>
{
    [JsonIgnore]
    public string GroupId { get; init; } = default!;
    public string Name { get; init; } = default!;
}