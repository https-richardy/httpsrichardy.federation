namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

[ExcludeFromCodeCoverage]
public static class ValidationExtension
{
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<AuthenticationCredentials>, AuthenticationCredentialsValidator>();
        services.AddTransient<IValidator<AuthorizationParameters>, AuthorizationParametersValidator>();
        services.AddTransient<IValidator<ClientAuthenticationCredentials>, ClientAuthenticationCredentialsValidator>();
        services.AddTransient<IValidator<IdentityEnrollmentCredentials>, IdentityEnrollmentCredentialsValidator>();

        services.AddTransient<IValidator<GroupCreationScheme>, GroupCreationValidator>();
        services.AddTransient<IValidator<GroupUpdateScheme>, GroupUpdateValidator>();
        services.AddTransient<IValidator<AssignGroupPermissionScheme>, AssignGroupPermissionValidator>();

        services.AddTransient<IValidator<PermissionCreationScheme>, PermissionCreationValidator>();
        services.AddTransient<IValidator<PermissionUpdateScheme>, PermissionUpdateValidator>();

        services.AddTransient<IValidator<RealmCreationScheme>, RealmCreationValidator>();
        services.AddTransient<IValidator<RealmUpdateScheme>, RealmUpdateValidator>();

        services.AddTransient<IValidator<AssignUserPermissionScheme>, AssignUserPermissionValidator>();
        services.AddTransient<IValidator<AssignRealmPermissionScheme>, AssignRealmPermissionValidator>();
        services.AddTransient<IValidator<ScopeCreationScheme>, ScopeCreationValidator>();
    }
}
