namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class SdkErrors
{
    public static readonly Error DeserializationFailure = new(
        Code: "#ERROR-40D17",
        Description: "Failed to deserialize the response payload. The response format may be invalid or unexpected."
    );

    public static readonly Error HttpRequestFailure = new(
        Code: "#ERROR-45AAE",
        Description: "The HTTP request failed due to a network error, timeout, or unexpected response."
    );

    public static readonly Error Unauthorized = new(
        Code: "#ERROR-7E560",
        Description: "The request was not authorized. Please check your credentials or realm configuration."
    );

    public static readonly Error Unknown = new(
        Code: "#ERROR-88B7D",
        Description: "An unknown error occurred in the SDK."
    );
}