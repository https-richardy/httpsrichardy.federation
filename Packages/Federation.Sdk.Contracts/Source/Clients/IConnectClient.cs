namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IConnectClient
{
    public Task<Result<ClientAuthenticationResult>> AuthenticateAsync(
        ClientAuthenticationCredentials credentials,
        CancellationToken cancellation = default
    );
}