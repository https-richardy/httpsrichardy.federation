namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record AuthenticationCredentials : IDispatchable<Result<AuthenticationResult>>
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}