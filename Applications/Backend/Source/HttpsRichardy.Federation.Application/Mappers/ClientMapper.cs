namespace HttpsRichardy.Federation.Application.Mappers;

public static class ClientMapper
{
    public static Client AsClient(this ClientCreationScheme client, ClientCredentials credentials, Realm realm) => new()
    {
        Name = client.Name,
        RealmId = realm.Id,
        ClientId = credentials.ClientId,
        Secret = credentials.ClientSecret,
        Flows = [.. client.Flows],
        RedirectUris = [.. client.RedirectUris.Select(uri => new RedirectUri(uri))]
    };

    public static ClientScheme AsResponse(this Client client) => new()
    {
        Id = client.Id,
        Name = client.Name,
        ClientId = client.ClientId,
        ClientSecret = client.Secret,
        Flows = client.Flows,
        RedirectUris = client.RedirectUris
    };
}
