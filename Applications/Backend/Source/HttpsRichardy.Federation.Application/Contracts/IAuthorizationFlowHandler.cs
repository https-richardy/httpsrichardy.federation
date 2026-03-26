namespace HttpsRichardy.Federation.Application.Contracts;

public interface IAuthorizationFlowHandler
{
    public Grant Grant { get; }


    public Task<Result<ClientAuthenticationResult>> HandleAsync(
        ClientAuthenticationCredentials parameters,
        CancellationToken cancellation = default
    );
}
