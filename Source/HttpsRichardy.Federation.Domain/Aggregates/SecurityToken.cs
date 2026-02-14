namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class SecurityToken : Aggregate
{
    public string Value { get; set; } = default!;
    public bool Revoked { get; set; }

    public string UserId { get; set; } = default!;
    public string RealmId { get; set; } = default!;

    public TokenType Type { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !Revoked && !IsExpired;
}
