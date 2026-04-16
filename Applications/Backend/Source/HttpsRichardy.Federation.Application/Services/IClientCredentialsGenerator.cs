namespace HttpsRichardy.Federation.Application.Services;

public interface IClientCredentialsGenerator
{
    public Task<ClientCredentials> GenerateAsync(string clientName, CancellationToken cancellation = default);
}
