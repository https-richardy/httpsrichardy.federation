namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Identity;

public sealed record AuthenticationResult
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
}