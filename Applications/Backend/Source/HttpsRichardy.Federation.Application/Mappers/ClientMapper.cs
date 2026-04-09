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
}
