namespace HttpsRichardy.Federation.Application.Handlers.Authorization;

public sealed class AuthorizationHandler(IRealmCollection realmCollection, IRedirectUriPolicy redirectUriPolicy) :
    IDispatchHandler<AuthorizationParameters, Result<AuthorizationScheme>>
{
    public async Task<Result<AuthorizationScheme>> HandleAsync(
        AuthorizationParameters parameters, CancellationToken cancellation = default)
    {
        var filters = new RealmFiltersBuilder()
            .WithClientId(parameters.ClientId)
            .Build();

        var clients = await realmCollection.GetRealmsAsync(filters, cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result<AuthorizationScheme>.Failure(RealmErrors.RealmDoesNotExist);
        }

        var redirectUri = parameters.RedirectUri.AsUri();

        // according to oauth 2.0 spec (RFC 6749, section 3.1.2.3):
        // https://datatracker.ietf.org/doc/html/rfc6749#section-3.1.2.3

        var redirectProof = await redirectUriPolicy.EnsureRedirectUriIsAllowedAsync(client, redirectUri, cancellation);
        if (redirectProof.IsFailure)
        {
            return Result<AuthorizationScheme>.Failure(redirectProof.Error);
        }

        return Result<AuthorizationScheme>.Success(parameters.AsReponse());
    }
}
