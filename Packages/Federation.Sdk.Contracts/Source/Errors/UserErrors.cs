namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class UserErrors
{
    public static readonly Error UserDoesNotExist = new(
        Code: "#ERROR-E6B32",
        Description: "The specified user does not exist."
    );

    public static readonly Error UserAlreadyInGroup = new(
        Code: "#ERROR-33066",
        Description: "The user is already a member of the specified group."
    );

    public static readonly Error UserAlreadyHasPermission = new(
        Code: "#ERROR-44DEC",
        Description: "The user already has the specified permission assigned."
    );

    public static readonly Error PermissionNotAssigned = new(
        Code: "#ERROR-C2FB0",
        Description: "The user does not have the specified permission assigned."
    );

    public static readonly Error UserNotInGroup = new(
        Code: "#ERROR-0E56E",
        Description: "The user is not a member of the specified group."
    );
}