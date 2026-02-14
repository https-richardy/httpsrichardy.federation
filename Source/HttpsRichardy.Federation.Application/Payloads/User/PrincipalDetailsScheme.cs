namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record PrincipalDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Username { get; init; } = default!;

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public IReadOnlyCollection<PermissionDetailsScheme> Permissions { get; init; } = [];
    public IReadOnlyCollection<GroupBasicDetailsScheme> Groups { get; init; } = [];
}
