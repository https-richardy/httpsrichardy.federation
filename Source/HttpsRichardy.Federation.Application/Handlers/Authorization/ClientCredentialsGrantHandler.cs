namespace HttpsRichardy.Federation.Application.Handlers.Authorization;

public sealed class ClientCredentialsGrantHandler(IRealmCollection realmCollection, ISecurityTokenService tokenService) : IAuthorizationFlowHandler
{
    public async Task<Result<ClientAuthenticationResult>> HandleAsync(ClientAuthenticationCredentials parameters, CancellationToken cancellation = default)
    {
        var filters = new RealmFiltersBuilder()
            .WithClientId(parameters.ClientId)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(filters, cancellation: cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.ClientNotFound);
        }

        if (parameters.ClientSecret != realm.SecretHash)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.InvalidClientCredentials);
        }

        var tokenResult = await tokenService.GenerateAccessTokenAsync(realm, cancellation);
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
