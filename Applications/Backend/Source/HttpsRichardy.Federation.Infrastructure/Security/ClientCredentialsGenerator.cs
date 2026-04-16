namespace HttpsRichardy.Federation.Infrastructure.Security;

public sealed class ClientCredentialsGenerator(IPasswordHasher passwordHasher) : IClientCredentialsGenerator
{
    public async Task<ClientCredentials> GenerateAsync(string clientName, CancellationToken cancellation = default)
    {
        var bytes = new byte[32];

        RandomNumberGenerator.Fill(bytes);

        var clientId = Convert.ToHexString(bytes).ToLowerInvariant();
        var clientSecret = await passwordHasher.HashPasswordAsync(clientId + clientName);

        return new ClientCredentials
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };
    }
}
