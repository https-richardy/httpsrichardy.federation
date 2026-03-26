namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Identity;

public sealed record IdentityEnrollmentCredentials
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}