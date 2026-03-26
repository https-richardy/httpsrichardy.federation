namespace HttpsRichardy.Federation.Sdk.Interceptors;

public sealed class AuthenticationInterceptor(IConnectClient connectClient, FederationOptions options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clientCredentials = new ClientAuthenticationCredentials
        {
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret
        };

        var result = await connectClient.AuthenticateAsync(clientCredentials, cancellationToken);
        if (result.IsFailure || result.Data is null)
        {
            throw new InvalidOperationException($"{result.Error.Code} - {result.Error.Description}");
        }

        var token = result.Data.AccessToken;

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
