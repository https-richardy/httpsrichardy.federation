namespace HttpsRichardy.Federation.Application.Services;

public interface IPasswordHasher
{
    public Task<string> HashPasswordAsync(string password);
    public Task<bool> VerifyPasswordAsync(string password, string hashedPassword);
}