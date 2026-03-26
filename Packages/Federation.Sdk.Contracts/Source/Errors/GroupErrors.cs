namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class GroupErrors
{
    public static readonly Error GroupAlreadyExists = new(
        Code: "#ERROR-1C6F3",
        Description: "The group with the specified name already exists."
    );

    public static readonly Error GroupAlreadyHasPermission = new(
        Code: "#ERROR-9C69E",
        Description: "The group already has the specified permission assigned."
    );

    public static readonly Error GroupDoesNotExist = new(
        Code: "#ERROR-4D2E2",
        Description: "The group with the specified ID does not exist."
    );

    public static readonly Error PermissionNotAssigned = new(
        Code: "#ERROR-C2FB0",
        Description: "The group does not have the specified permission assigned."
    );
}