namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 70_000;

    public Task<string> HashPasswordAsync(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = DeriveKey(password, salt);
        var result = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";

        return Task.FromResult(result);
    }

    public Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 2)
        {
            return Task.FromResult(false);
        }

        var salt = Convert.FromBase64String(parts[0]);
        var storedKey = Convert.FromBase64String(parts[1]);

        var derivedKey = DeriveKey(password, salt);
        var result = CryptographicOperations.FixedTimeEquals(derivedKey, storedKey);

        return Task.FromResult(result);
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize);
    }
}