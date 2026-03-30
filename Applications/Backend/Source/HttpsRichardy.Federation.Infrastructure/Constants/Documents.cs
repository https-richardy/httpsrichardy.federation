namespace HttpsRichardy.Federation.Infrastructure.Constants;

public static class Documents
{
    public static class User
    {
        public const string IsDeleted = nameof(Domain.Aggregates.User.IsDeleted);
        public const string Username = nameof(Domain.Aggregates.User.Username);
        public const string RealmId = nameof(Domain.Aggregates.User.RealmId);
        public const string Id = "_id";
    }

    public static class Permission
    {
        public const string Name = nameof(Domain.Aggregates.Permission.Name);
        public const string Description = nameof(Domain.Aggregates.Permission.Description);
        public const string IsDeleted = nameof(Domain.Aggregates.Permission.IsDeleted);
        public const string RealmId = nameof(Domain.Aggregates.Permission.RealmId);
        public const string Id = "_id";
    }

    public static class Group
    {
        public const string Id = "_id";
        public const string Name = nameof(Domain.Aggregates.Group.Name);
        public const string RealmId = nameof(Domain.Aggregates.Group.RealmId);
        public const string IsDeleted = nameof(Domain.Aggregates.Group.IsDeleted);
    }

    public static class Realm
    {
        public const string Name = nameof(Domain.Aggregates.Realm.Name);
        public const string ClientId = nameof(Domain.Aggregates.Realm.ClientId);
        public const string IsDeleted = nameof(Domain.Aggregates.Realm.IsDeleted);
        public const string Id = "_id";
    }

    public static class SecurityToken
    {
        public const string Value = nameof(Domain.Aggregates.SecurityToken.Value);
        public const string Type = nameof(Domain.Aggregates.SecurityToken.Type);
        public const string UserId = nameof(Domain.Aggregates.SecurityToken.UserId);
        public const string RealmId = nameof(Domain.Aggregates.SecurityToken.RealmId);
        public const string IsDeleted = nameof(Domain.Aggregates.SecurityToken.IsDeleted);
        public const string Id = "_id";
    }
}
