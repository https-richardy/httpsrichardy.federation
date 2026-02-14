namespace HttpsRichardy.Federation.Application.Contracts;

public interface IAuthorizationFlowHandler
{
    public Task<Result<ClientAuthenticationResult>> HandleAsync(
        ClientAuthenticationCredentials parameters,
        CancellationToken cancellation = default
    );
}
