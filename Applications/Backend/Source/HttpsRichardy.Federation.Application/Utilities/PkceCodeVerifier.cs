namespace HttpsRichardy.Federation.Application.Utilities;

public static class PkceCodeVerifier
{
    public static bool Validate(string codeVerifier, string codeChallenge, string method)
    {
        if (string.IsNullOrWhiteSpace(codeVerifier) || string.IsNullOrWhiteSpace(codeChallenge))
        {
            return false;
        }

        // according to pkce spec (RFC 7636, section 4.6):
        // https://datatracker.ietf.org/doc/html/rfc7636#section-4.6

        return method switch
        {
            SupportedPkceMethods.PkceS256 => PkceCodeVerifier.ValidateS256(codeVerifier, codeChallenge),
            SupportedPkceMethods.PkcePlain => codeVerifier == codeChallenge,

            _ => false
        };
    }

    private static bool ValidateS256(string codeVerifier, string codeChallenge)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(codeVerifier));
        var hashed = Base64UrlEncoder.Encode(bytes);

        return hashed == codeChallenge;
    }
}
