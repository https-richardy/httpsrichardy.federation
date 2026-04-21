namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class AuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            IssuerSigningKeyResolver = (token, _, kid, _) =>
            {
                var context = accessor.HttpContext;
                if (context is null || string.IsNullOrWhiteSpace(token))
                    return [];

                var secretCollection = context.RequestServices.GetRequiredService<ISecretCollection>();
                var realmCollection = context.RequestServices.GetRequiredService<IRealmCollection>();

                var jsonWebToken = new Microsoft.IdentityModel.JsonWebTokens.JsonWebToken(token);
                var realmId = jsonWebToken.Claims.FirstOrDefault(claim => claim.Type == Infrastructure.Constants.IdentityClaimNames.RealmId)?.Value;

                if (string.IsNullOrWhiteSpace(realmId))
                {
                    var realmName = jsonWebToken.Claims.FirstOrDefault(claim => claim.Type == Infrastructure.Constants.IdentityClaimNames.Realm)?.Value;

                    if (string.IsNullOrWhiteSpace(realmName))
                        return [];

                    var realmFilters = RealmFilters.WithSpecifications()
                        .WithName(realmName)
                        .Build();

                    var realms = realmCollection
                        .GetRealmsAsync(realmFilters, context.RequestAborted)
                        .GetAwaiter()
                        .GetResult();

                    realmId = realms.FirstOrDefault()?.Id;
                }

                if (string.IsNullOrWhiteSpace(realmId))
                    return [];

                var secretFilters = SecretFilters.WithSpecifications()
                    .WithRealm(realmId)
                    .WithCanValidate()
                    .Build();

                var secrets = secretCollection
                    .GetSecretsAsync(secretFilters, context.RequestAborted)
                    .GetAwaiter()
                    .GetResult();

                if (!string.IsNullOrWhiteSpace(kid))
                {
                    secrets = [.. secrets.Where(secret => secret.Id == kid)];
                }

                return [.. secrets.Select(secret =>
                {
                    var key = Common.Utilities.RsaHelper.CreateSecurityKeyFromPublicKey(secret.PublicKey);
                    key.KeyId = secret.Id;

                    return key;
                })];
            }
        };

        var builder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        builder.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = validationParameters;
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.Events = Authentication.Events;
        });

        return services;
    }
}
