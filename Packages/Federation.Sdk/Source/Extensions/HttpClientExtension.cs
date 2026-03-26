namespace HttpsRichardy.Federation.Sdk.Extensions;

public static class HttpClientExtensions
{
    public static HttpClient WithRealm(this HttpClient client, string realm)
    {
        if (client.DefaultRequestHeaders.Contains("realm"))
            client.DefaultRequestHeaders.Remove("realm");

        client.DefaultRequestHeaders.Add("realm", realm);

        return client;
    }

    public static HttpClient WithAuthorization(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }
}
