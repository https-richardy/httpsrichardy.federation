namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record SessionTokenRenewalScheme : IDispatchable<Result<AuthenticationResult>>
{
    public string RefreshToken { get; init; } = default!;
}