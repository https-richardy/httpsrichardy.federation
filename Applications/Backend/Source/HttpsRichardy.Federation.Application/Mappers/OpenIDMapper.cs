namespace HttpsRichardy.Federation.Application.Mappers;

public static class ConnectMapper
{
    public static OpenIDConfigurationScheme AsConfiguration(Uri baseUri)
    {
        var issuer = baseUri.GetLeftPart(UriPartial.Authority);
        var authorizeUri = new Uri(baseUri, OpenIDEndpoints.Authorize);
        var tokenUri = new Uri(baseUri, OpenIDEndpoints.Token);
        var userInfoUri = new Uri(baseUri, OpenIDEndpoints.UserInfo);
        var jwksUri = new Uri(baseUri, OpenIDEndpoints.Jwks);

        var configuration = new OpenIDConfigurationScheme
        {
            Issuer = issuer,
            AuthorizationEndpoint = authorizeUri.ToString(),
            TokenEndpoint = tokenUri.ToString(),
            UserInfoEndpoint = userInfoUri.ToString(),
            JwksUri = jwksUri.ToString()
        };

        return configuration;
    }
}
