namespace HttpsRichardy.Federation.Proxy;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();

        builder.Services
            .AddOcelot(builder.Configuration)
            .AddPolly();

        var app = builder.Build();

        app.UseHttpsRedirection();

        await app.UseOcelot();
        await app.RunAsync();
    }
}
