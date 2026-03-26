namespace HttpsRichardy.Federation.Application.Utilities;

public static class Base64UrlEncoder
{
    public static string Encode(byte[] input)
    {
        if (input == null || input.Length == 0)
            return string.Empty;

        var base64 = Convert.ToBase64String(input);

        return base64.Replace("+", "-")
                     .Replace("/", "_")
                     .TrimEnd('=');
    }

    public static byte[] Decode(string input)
    {
        if (string.IsNullOrEmpty(input))
            return [];

        var base64 = input.Replace("-", "+")
                          .Replace("_", "/");

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}