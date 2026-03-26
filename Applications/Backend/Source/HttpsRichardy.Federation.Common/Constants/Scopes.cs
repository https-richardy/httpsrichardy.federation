namespace HttpsRichardy.Federation.Common.Constants;

public static class Scopes
{
    public static class OpenID
    {
        public const string Name = "openid";
        public const string Description = "Authenticate using OpenID Connect";
    }

    public static class Profile
    {
        public const string Name = "profile";
        public const string Description = "Access basic profile information (name, family name, etc.)";
    }

    public static class Email
    {
        public const string Name = "email";
        public const string Description = "Access the user's email address";
    }

    public static class Address
    {
        public const string Name = "address";
        public const string Description = "Access the user's address information";
    }

    public static class Phone
    {
        public const string Name = "phone";
        public const string Description = "Access the user's phone number";
    }
}