namespace HttpsRichardy.Federation.Common.Utilities;

public static class RsaHelper
{
    public static RSA CreateRsaFromPrivateKey(string base64PrivateKey)
    {
        var rsa = RSA.Create();

        rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey), out _);

        return rsa;
    }

    public static RSA CreateRsaFromPublicKey(string base64PublicKey)
    {
        var rsa = RSA.Create();

        rsa.ImportRSAPublicKey(Convert.FromBase64String(base64PublicKey), out _);

        return rsa;
    }

    public static RsaSecurityKey CreateSecurityKeyFromPrivateKey(string base64PrivateKey)
    {
        var rsa = CreateRsaFromPrivateKey(base64PrivateKey);

        return new RsaSecurityKey(rsa);
    }

    public static RsaSecurityKey CreateSecurityKeyFromPublicKey(string base64PublicKey)
    {
        var rsa = CreateRsaFromPublicKey(base64PublicKey);

        return new RsaSecurityKey(rsa);
    }
}
