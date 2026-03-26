namespace HttpsRichardy.Federation.Application.Mappers;

public static class JsonWebKeysMapper
{
    public static JsonWebKeyScheme AsJsonWebKeys(Secret secret)
    {
        var publicKey = Common.Utilities.RsaHelper.CreateRsaFromPublicKey(secret.PublicKey);
        var parameters = publicKey.ExportParameters(false);

        return new JsonWebKeyScheme
        {
            Identifier = secret.Id,
            Exponent = Base64UrlEncoder.Encode(parameters.Exponent!),
            Modulus = Base64UrlEncoder.Encode(parameters.Modulus!)
        };
    }

    public static JsonWebKeySetScheme AsJsonWebKeySetScheme(Secret secret)
    {
        return new JsonWebKeySetScheme
        {
            Keys = [JsonWebKeysMapper.AsJsonWebKeys(secret)]
        };
    }
}
