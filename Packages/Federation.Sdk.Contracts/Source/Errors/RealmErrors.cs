namespace HttpsRichardy.Federation.Sdk.Contracts.Errors;

public static class RealmErrors
{
    public static readonly Error HttpContextUnavailable = new(
        Code: "#ERROR-8B1C5",
        Description: "No HTTP context available to retrieve realm information."
    );

    public static readonly Error RealmDoesNotExist = new(
        Code: "#ERROR-2FB9A",
        Description: "The specified realm does not exist."
    );

    public static readonly Error RealmHeaderMissing = new(
        Code: "#ERROR-B2E42",
        Description: "Realm header is missing from the HTTP request."
    );

    public static readonly Error RealmAlreadyExists = new(
        Code: "#ERROR-F98CE",
        Description: "A realm with the same name already exists."
    );

    public static readonly Error RealmAlreadyHasPermission = new(
        Code: "#ERROR-F23E2",
        Description: "The realm already has the specified permission assigned."
    );

    public static readonly Error PermissionNotAssigned = new(
        Code: "#ERROR-C2FB0",
        Description: "The realm does not have the specified permission assigned."
    );
}