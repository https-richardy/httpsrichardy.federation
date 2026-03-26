namespace HttpsRichardy.Federation.Sdk.Contracts.Payloads.Group;

public sealed record AssignGroupPermission
{
    [JsonIgnore]
    public string GroupId { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
}
