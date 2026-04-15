namespace HttpsRichardy.Federation.Domain.Errors;

public static class ClientErrors
{
    public static readonly Error ClientDoesNotExist = new(
        Code: "#ERROR-2D943",
        Description: "The client with the specified ID does not exist."
    );
}
