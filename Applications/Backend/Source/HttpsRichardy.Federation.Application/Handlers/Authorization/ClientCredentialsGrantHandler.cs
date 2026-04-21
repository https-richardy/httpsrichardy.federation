namespace HttpsRichardy.Federation.Application.Handlers.Authorization;

public sealed class ClientCredentialsGrantHandler(IClientCollection clientCollection, ISecurityTokenService tokenService) :
    IAuthorizationFlowHandler
{
    public Grant Grant => Grant.ClientCredentials;

    public async Task<Result<ClientAuthenticationResult>> HandleAsync(ClientAuthenticationCredentials parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithClientId(parameters.ClientId)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, cancellation: cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.ClientNotFound);
        }

        if (parameters.ClientSecret != client.Secret)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.InvalidClientCredentials);
        }

        var tokenResult = await tokenService.GenerateAccessTokenAsync(client, cancellation);
        if (tokenResult.IsFailure)
        {
            return Result<ClientAuthenticationResult>.Failure(tokenResult.Error);
        }

        var response = new ClientAuthenticationResult
        {
            AccessToken = tokenResult.Data!.Value
        };

        return Result<ClientAuthenticationResult>.Success(response);
    }
}
