namespace HttpsRichardy.Federation.Sdk.Interceptors;

public sealed class RealmInterceptor(FederationOptions options) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Remove("realm");
        request.Headers.Add("realm", options.Realm);

        return await base.SendAsync(request, cancellationToken);
    }
}