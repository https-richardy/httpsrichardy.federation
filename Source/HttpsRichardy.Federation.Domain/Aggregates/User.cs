namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class User : Aggregate
{
    public string RealmId { get; set; } = default!;
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public ICollection<Permission> Permissions { get; set; } = [];
    public ICollection<Group> Groups { get; set; } = [];
}