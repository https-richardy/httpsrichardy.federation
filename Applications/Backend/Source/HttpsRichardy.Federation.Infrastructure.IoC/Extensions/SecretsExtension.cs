namespace HttpsRichardy.Federation.Infrastructure.IoC.Extensions;

public static class SecretsExtension
{
    public static void AddInitialSecrets(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var secretRepository = serviceProvider.GetRequiredService<ISecretCollection>();

        var secret = secretRepository.GetSecretAsync()
            .GetAwaiter()
            .GetResult();

        /* if no secret exists, generate an initial one to sign JWT tokens */
        if (secret is null)
        {
            using var rsa = RSA.Create(2048);

            secret = new Secret
            {
                PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
                PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey())
            };

            secretRepository.InsertAsync(secret)
                .GetAwaiter()
                .GetResult();
        }
    }
}
