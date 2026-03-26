namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidTokenFormat = new(
        Code: "#ERROR-32B37",
        Description: "The token format is invalid or the token is malformed."
    );

    public static readonly Error TokenExpired = new(
        Code: "#ERROR-5F736",
        Description: "The token has expired."
    );

    public static readonly Error InvalidSignature = new(
        Code: "#ERROR-FB8E4",
        Description: "The token signature is invalid."
    );

    public static readonly Error InvalidRefreshToken = new(
        Code: "#ERROR-2C0D9",
        Description: "The provided refresh token is invalid, expired, or has already been used."
    );

    public static readonly Error LogoutFailed = new(
        Code: "#ERROR-60CBC",
        Description: "Logout failed: the refresh token is invalid, expired, or has already been used."
    );

    public static readonly Error InvalidCredentials = new(
        Code: "#ERROR-A7E7C",
        Description: "The provided credentials are invalid."
    );

    public static readonly Error ClientNotFound = new(
        Code: "#ERROR-0AF50",
        Description: "The client was not found."
    );

    public static readonly Error InvalidClientCredentials = new(
        Code: "#ERROR-D5D7C",
        Description: "The provided client credentials are invalid."
    );

    public static readonly Error UserNotFound = new(
        Code: "#ERROR-04A2F",
        Description: "The user was not found."
    );
}
