namespace HttpsRichardy.Federation.Application.Mappers;

public static class TokenMapper
{
    public static SecurityToken AsRefreshToken(string token) => new()
    {
        Value = token,
        Type = TokenType.Refresh
    };

    public static SecurityToken AsEmailVerificationToken(string token) => new()
    {
        Value = token,
        Type = TokenType.EmailVerification
    };

    public static SecurityToken AsPasswordResetToken(string token) => new()
    {
        Value = token,
        Type = TokenType.PasswordReset
    };
}