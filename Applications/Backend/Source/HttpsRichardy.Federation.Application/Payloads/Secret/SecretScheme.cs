namespace HttpsRichardy.Federation.Application.Payloads.Secret;

public sealed record SecretScheme
{
    public string Id { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
}
