namespace HttpsRichardy.Federation.Application.Mappers;

public static class RedirectUriMapper
{
    public static RedirectUri AsUri(this string uri) =>
        new(uri);
}
