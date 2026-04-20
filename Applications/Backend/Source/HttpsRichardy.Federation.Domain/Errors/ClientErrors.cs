namespace HttpsRichardy.Federation.Domain.Errors;

public static class ClientErrors
{
    public static readonly Error ClientAlreadyExists = new(
        Code: "#ERROR-A7C31",
        Description: "A client with the same name already exists."
    );

    public static readonly Error ClientAlreadyHasPermission = new(
        Code: "#ERROR-8D71B",
        Description: "The client already has the specified permission assigned."
    );

    public static readonly Error ClientAlreadyHasAudience = new(
        Code: "#ERROR-F4E2A",
        Description: "The client already has the specified audience assigned."
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
