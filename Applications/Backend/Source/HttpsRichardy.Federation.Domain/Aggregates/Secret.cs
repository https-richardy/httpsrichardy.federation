namespace HttpsRichardy.Federation.Domain.Aggregates;

public sealed class Secret : Aggregate
{
    public string PrivateKey { get; set; } = default!;
    public string PublicKey { get; set; } = default!;
    public string RealmId { get; set; } = default!;

    public DateTime? ExpiresAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
}
