namespace HttpsRichardy.Federation.Application.Payloads.Client;

public sealed record ClientDeletionScheme : IDispatchable<Result>
{
    public string Id { get; init; } = default!;
}
