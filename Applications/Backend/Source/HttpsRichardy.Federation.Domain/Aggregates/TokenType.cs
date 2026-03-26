namespace HttpsRichardy.Federation.Domain.Aggregates;

public enum TokenType
{
    Refresh,
    EmailVerification,
    AuthorizationCode,
    PasswordReset
}
