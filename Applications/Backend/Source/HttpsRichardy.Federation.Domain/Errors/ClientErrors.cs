namespace HttpsRichardy.Federation.Domain.Errors;

public static class ClientErrors
{
    public static readonly Error ClientAlreadyHasPermission = new(
        Code: "#ERROR-8D71B",
        Description: "The client already has the specified permission assigned."
    );

    public static readonly Error PermissionNotAssigned = new(
        Code: "#ERROR-C2FB0",
        Description: "The client does not have the specified permission assigned."
    );

    public static readonly Error ClientDoesNotExist = new(
        Code: "#ERROR-2D943",
        Description: "The client with the specified ID does not exist."
    );
}
