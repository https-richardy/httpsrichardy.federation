namespace HttpsRichardy.Federation.Application.Handlers.Identity;

public sealed class ClientAuthenticationHandler(IEnumerable<IAuthorizationFlowHandler> handlers) :
    IDispatchHandler<ClientAuthenticationCredentials, Result<ClientAuthenticationResult>>
{
    public async Task<Result<ClientAuthenticationResult>> HandleAsync(
        ClientAuthenticationCredentials parameters, CancellationToken cancellation = default)
    {
        var grant = parameters.GrantType switch
        {
            SupportedGrantType.ClientCredentials => Grant.ClientCredentials,
            SupportedGrantType.AuthorizationCode => Grant.AuthorizationCode,

            _ => Grant.Unspecified
        };

        var handler = handlers.FirstOrDefault(handler => handler.Grant == grant);
        if (handler is null)
        {
            return Result<ClientAuthenticationResult>.Failure(AuthorizationErrors.UnsupportedGrant);
        }

        return await handler.HandleAsync(parameters, cancellation);
    }
}
