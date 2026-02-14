namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record UserDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Username { get; init; } = default!;
}