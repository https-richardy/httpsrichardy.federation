namespace HttpsRichardy.Federation.Sdk.Extensions;

public static class FederationExtension
{
    public static void AddFederation(this IServiceCollection services, Action<FederationOptions> configure)
    {
        var options = new FederationOptions();

        configure(options);

        services.AddTransient<AuthenticationInterceptor>();
        services.AddTransient<RealmInterceptor>();

        services.AddSingleton(options);
        services.AddHttpClient<IConnectClient, ConnectClient>(client =>
        {
            client.BaseAddress = new Uri(options.Authority);
        });

        services.AddSdkHttpClient<IIdentityClient, IdentityClient>(options.Authority)
            .WithRealmInterceptor()
            .WithAuthenticationInterceptor();

        services.AddSdkHttpClient<IPermissionsClient, PermissionsClient>(options.Authority)
            .WithRealmInterceptor()
            .WithAuthenticationInterceptor();

        services.AddSdkHttpClient<IGroupsClient, GroupsClient>(options.Authority)
            .WithRealmInterceptor()
            .WithAuthenticationInterceptor();

        services.AddSdkHttpClient<IRealmsClient, RealmsClient>(options.Authority)
            .WithRealmInterceptor()
            .WithAuthenticationInterceptor();

        services.AddSdkHttpClient<IUsersClient, UsersClient>(options.Authority)
            .WithRealmInterceptor()
            .WithAuthenticationInterceptor();

        services.AddBearerAuthentication();
        services.AddAuthorization();
    }
}