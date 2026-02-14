namespace HttpsRichardy.Federation.Application.Handlers.Authorization;

public sealed class AuthorizationCodeGrantHandler(IRealmCollection realmCollection, IUserCollection userCollection, ISecurityTokenService tokenService, ITokenCollection tokenCollection) : IAuthorizationFlowHandler
{
    public async Task<Result<ClientAuthenticationResult>> HandleAsync(
        ClientAuthenticationCredentials parameters, CancellationToken cancellation = default)
    {
        var filters = new TokenFiltersBuilder()
            .WithValue(parameters.Code)
            .WithType(TokenType.AuthorizationCode)
            .Build();

        var tokens = await tokenCollection.GetTokensAsync(filters, cancellation: cancellation);
        var token = tokens.FirstOrDefault();

        if (token is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthorizationErrors.InvalidAuthorizationCode);
        }

        if (token.IsExpired)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthorizationErrors.AuthorizationCodeExpired);
        }

        var realmFilters = new RealmFiltersBuilder()
            .WithIdentifier(token.RealmId)
            .Build();

        var realms = await realmCollection.GetRealmsAsync(realmFilters, cancellation: cancellation);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.ClientNotFound);
        }

        var codeChallenge = token.Metadata.GetValueOrDefault("code.challenge")!;
        var codeChallengeMethod = token.Metadata.GetValueOrDefault("code.challenge.method")!;

        if (!PkceCodeVerifier.Validate(parameters.CodeVerifier, codeChallenge, codeChallengeMethod))
        {
            return Result<ClientAuthenticationResult>.Failure(AuthorizationErrors.InvalidCodeVerifier);
        }

        var userFilters = new UserFiltersBuilder()
            .WithIdentifier(token.UserId)
            .Build();

        var users = await userCollection.GetUsersAsync(userFilters, cancellation: cancellation);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthenticationErrors.UserNotFound);
        }

        var tokenResult = await tokenService.GenerateAccessTokenAsync(user, cancellation);
        if (tokenResult.IsFailure || tokenResult.Data is null)
        {
            return Result<ClientAuthenticationResult>.Failure(tokenResult.Error);
        }

        return Result<ClientAuthenticationResult>.Success(new()
        {
            AccessToken = tokenResult.Data.Value
        });
    }
}
