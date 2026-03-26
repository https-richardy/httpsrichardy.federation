namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class AuthorizationErrors
{
    public static readonly Error RedirectUriNotAllowed = new(
        Code: "#ERROR-F8EBC",
        Description: "The specified redirect URI is not registered or allowed for this realm."
    );

    public static readonly Error InvalidAuthorizationCode = new(
        Code: "#ERROR-C9D0A",
        Description: "The provided authorization code is invalid, expired, or has already been used."
    );

    public static readonly Error AuthorizationCodeExpired = new(
        Code: "#ERROR-5F5B3",
        Description: "The authorization code has expired."
    );

    public static readonly Error InvalidCodeVerifier = new(
        Code: "#ERROR-DDA70",
        Description: "The provided code verifier does not match the code challenge."
    );
}
