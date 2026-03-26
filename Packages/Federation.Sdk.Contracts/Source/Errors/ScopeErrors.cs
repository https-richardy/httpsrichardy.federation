namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class ScopeErrors
{
    public static readonly Error ScopeAlreadyExists = new(
        Code: "#ERROR-8D128",
        Description: "The scope with the specified name already exists."
    );

    public static readonly Error ScopeDoesNotExists = new(
        Code: "#ERROR-903F9",
        Description: "The scope with the specified name does not exist."
    );
}