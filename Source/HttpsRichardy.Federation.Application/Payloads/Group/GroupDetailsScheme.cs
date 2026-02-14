namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupDetailsScheme
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public IReadOnlyCollection<PermissionDetailsScheme> Permissions { get; init; } = [];
}