namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record RemoveUserFromGroupScheme : IDispatchable<Result>
{
    public string UserId { get; init; } = default!;
    public string GroupId { get; init; } = default!;
}
