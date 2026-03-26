namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupCreationScheme : IDispatchable<Result<GroupDetailsScheme>>
{
    public string Name { get; init; } = default!;
}