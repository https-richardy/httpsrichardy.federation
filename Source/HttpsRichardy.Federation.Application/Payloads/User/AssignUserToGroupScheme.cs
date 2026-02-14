namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record AssignUserToGroupScheme : IDispatchable<Result>
{
    [JsonIgnore]
    public string UserId { get; init; } = default!;
    public string GroupId { get; init; } = default!;
}