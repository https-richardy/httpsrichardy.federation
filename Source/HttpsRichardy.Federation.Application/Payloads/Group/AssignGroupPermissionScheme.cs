namespace HttpsRichardy.Federation.Application.Payloads.Group;

public sealed record AssignGroupPermissionScheme : IDispatchable<Result<GroupDetailsScheme>>
{
    [JsonIgnore]
    public string GroupId { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
}
