namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class PermissionErrors
{
    public static readonly Error PermissionAlreadyExists = new(
        Code: "#ERROR-93F4A",
        Description: "The permission with the specified name already exists."
    );

    public static readonly Error PermissionDoesNotExist = new(
        Code: "#ERROR-93697",
        Description: "The specified permission does not exist."
    );
}
