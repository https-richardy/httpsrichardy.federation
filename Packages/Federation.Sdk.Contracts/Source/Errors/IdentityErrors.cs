namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class IdentityErrors
{
    public static readonly Error UserAlreadyExists = new(
        Code: "#ERROR-DC3B1",
        Description: "The user with the specified username already exists."
    );
}
