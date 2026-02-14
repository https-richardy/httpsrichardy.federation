namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record UserDeletionScheme : IDispatchable<Result>
{
    public string UserId { get; init; } = default!;
}