namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record IdentityEnrollmentCredentials : IDispatchable<Result<UserDetailsScheme>>
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}