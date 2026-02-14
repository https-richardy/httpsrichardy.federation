namespace HttpsRichardy.Federation.Application.Payloads.Authorization;

public sealed record AuthorizationParameters : IDispatchable<Result<AuthorizationScheme>>
{
    public string ResponseType { get; init; } = default!;
    public string RedirectUri { get; init; } = default!;

    public string ClientId { get; init; } = default!;

    public string CodeChallenge { get; init; } = default!;
    public string CodeChallengeMethod { get; init; } = default!;

    public string? Scope { get; init; } = default!;
    public string? State { get; init; } = default!;
}
