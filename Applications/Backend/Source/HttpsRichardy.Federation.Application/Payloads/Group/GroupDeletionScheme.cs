namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record GroupDeletionScheme : IDispatchable<Result>
{
    public string GroupId { get; init; } = default!;
}
