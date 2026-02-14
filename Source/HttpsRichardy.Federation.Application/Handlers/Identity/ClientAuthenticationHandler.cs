namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class ClientAuthenticationHandler(IRealmCollection realmCollection, IUserCollection userCollection, ITokenCollection tokenCollection, ISecurityTokenService tokenService) :
    IDispatchHandler<ClientAuthenticationCredentials, Result<ClientAuthenticationResult>>
{
    public async Task<Result<ClientAuthenticationResult>> HandleAsync(
        ClientAuthenticationCredentials parameters, CancellationToken cancellation = default)
    {
        IAuthorizationFlowHandler handler = parameters.GrantType switch
        {
            SupportedGrantType.AuthorizationCode => new AuthorizationCodeGrantHandler(realmCollection, userCollection, tokenService, tokenCollection),
            SupportedGrantType.ClientCredentials => new ClientCredentialsGrantHandler(realmCollection, tokenService),
        };

        return await handler.HandleAsync(parameters, cancellation);
    }
}
