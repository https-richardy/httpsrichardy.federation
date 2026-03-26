namespace HttpsRichardy.Federation.Sdk.TestSuite.Clients;

public sealed class RealmsClientTests(FederationProviderFixture server) :
    IClassFixture<FederationProviderFixture>
{
    private readonly HttpClient _httpClient = server.HttpClient;

    [Fact(DisplayName = "[e2e] - when create realm with valid data should succeed")]
    public async Task WhenCreateRealm_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client and the realm to create */
        var realmsClient = new RealmsClient(_httpClient);
        var realm = new RealmCreationScheme
        {
            Name = "federation.defaults.realms.testing",
            Description = "Realm for testing purposes"
        };

        /* act: call the create realm async method */
        var result = await realmsClient.CreateRealmAsync(realm);

        /* assert: verify that the realm was created successfully */
        Assert.True(result.IsSuccess);

        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Id);

        Assert.Equal(realm.Name, result.Data.Name);
        Assert.Equal(realm.Description, result.Data.Description);
    }

    [Fact(DisplayName = "[e2e] - when create realm with existing name should fail")]
    public async Task WhenCreateRealm_WithExistingName_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client and the realm to create */
        var realmsClient = new RealmsClient(_httpClient);
        var realm = new RealmCreationScheme
        {
            Name = "federation.defaults.realms.existing",
            Description = "Realm for testing purposes"
        };

        /* act: call the create realm async method twice */
        await realmsClient.CreateRealmAsync(realm);

        var result = await realmsClient.CreateRealmAsync(realm);

        /* assert: verify that the second creation failed with RealmAlreadyExists error */
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);

        Assert.Equal(RealmErrors.RealmAlreadyExists, result.Error);
    }

    [Fact(DisplayName = "[e2e] - when update realm with valid data should succeed")]
    public async Task WhenUpdateRealm_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client and the realm to create */
        var realmsClient = new RealmsClient(_httpClient);
        var realmToCreate = new RealmCreationScheme
        {
            Name = "federation.defaults.realms.to.update",
            Description = "Realm to be updated"
        };

        var createResult = await realmsClient.CreateRealmAsync(realmToCreate);

        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* arrange: prepare update context with the created realm Id and new name/description */
        var payload = new RealmUpdateScheme
        {
            RealmId = createResult.Data.Id,
            Name = "federation.defaults.realms.updated",
            Description = "Updated realm description"
        };

        /* act: call the update realm async method */
        var updateResult = await realmsClient.UpdateRealmAsync(payload);

        /* assert: verify that the realm was updated successfully */
        Assert.True(updateResult.IsSuccess);
        Assert.NotNull(updateResult.Data);

        Assert.Equal(createResult.Data.Id, updateResult.Data.Id);
        Assert.Equal(payload.Name, updateResult.Data.Name);
        Assert.Equal(payload.Description, updateResult.Data.Description);
    }

    [Fact(DisplayName = "[e2e] - when update non-existent realm should fail")]
    public async Task WhenUpdateNonExistentRealm_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client and prepare update context for a non-existent realm */
        var realmsClient = new RealmsClient(_httpClient);
        var realmToUpdate = new RealmUpdateScheme
        {
            RealmId = "realm_Jdahsdn18781263",
            Name = "federation.defaults.realms.non.existent",
            Description = "non-existent realm"
        };

        /* act: call the update realm async method */
        var updateResult = await realmsClient.UpdateRealmAsync(realmToUpdate);

        /* assert: verify that the update failed and the correct error was returned */
        Assert.True(updateResult.IsFailure);

        Assert.NotNull(updateResult.Error);
        Assert.Equal(RealmErrors.RealmDoesNotExist, updateResult.Error);
    }

    [Fact(DisplayName = "[e2e] - when delete realm with valid data should succeed")]
    public async Task WhenDeleteRealm_WithValidData_ShouldSucceed()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client and the realm to create */
        var realmsClient = new RealmsClient(_httpClient);
        var realmToCreate = new RealmCreationScheme
        {
            Name = "federation.defaults.realms.to.delete",
            Description = "Realm to be deleted"
        };

        /* act: call the create realm async method */
        var createResult = await realmsClient.CreateRealmAsync(realmToCreate);

        /* assert: verify that the realm was created successfully */
        Assert.True(createResult.IsSuccess);
        Assert.NotNull(createResult.Data);

        /* act: call the delete realm async method */
        var deleteResult = await realmsClient.DeleteRealmAsync(createResult.Data.Id);

        /* assert: verify that the realm was deleted successfully */
        Assert.True(deleteResult.IsSuccess);
    }

    [Fact(DisplayName = "[e2e] - when delete non-existent realm should fail")]
    public async Task WhenDeleteNonExistentRealm_ShouldFail()
    {
        /* arrange: create an identity client with the proper realm header and define admin credentials */
        var identityClient = new IdentityClient(_httpClient.WithRealm("master"));
        var credentials = new AuthenticationCredentials
        {
            Username = "admin",
            Password = "admin"
        };

        /* act: send a POST request to the authenticate endpoint using the identity client */
        var authenticationResult = await identityClient.AuthenticateAsync(credentials);

        /* assert: ensure the authentication was successful and the result contains data */
        Assert.True(authenticationResult.IsSuccess);
        Assert.NotNull(authenticationResult.Data);

        _httpClient.WithAuthorization(authenticationResult.Data.AccessToken);

        /* arrange: create the realms client */
        var realmsClient = new RealmsClient(_httpClient);
        var nonExistentRealmId = "realm_Jdahsdn18781263";

        /* act: call the delete realm async method */
        var deleteResult = await realmsClient.DeleteRealmAsync(nonExistentRealmId);

        /* assert: verify that the delete failed and the correct error was returned */
        Assert.False(deleteResult.IsSuccess);

        Assert.NotNull(deleteResult.Error);
        Assert.Equal(RealmErrors.RealmDoesNotExist, deleteResult.Error);
    }
}