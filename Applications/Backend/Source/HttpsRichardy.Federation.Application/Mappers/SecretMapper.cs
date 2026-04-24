namespace HttpsRichardy.Federation.Application.Mappers;

public static class SecretMapper
{
    public static SecretScheme AsResponse(this Secret secret) => new()
    {
        Id = secret.Id,
        CreatedAt = secret.CreatedAt,
        ExpiresAt = secret.ExpiresAt,
        GracePeriodEndsAt = secret.GracePeriodEndsAt
    };
}
