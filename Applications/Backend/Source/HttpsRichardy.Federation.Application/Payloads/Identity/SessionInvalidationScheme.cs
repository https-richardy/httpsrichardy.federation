namespace HttpsRichardy.Federation.Application.Payloads.Identity;

public sealed record SessionInvalidationScheme : IDispatchable<Result>
{
    public string RefreshToken { get; init; } = default!;
}