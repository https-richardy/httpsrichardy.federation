namespace HttpsRichardy.Federation.Proxy;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseAuthorization();

        await app.RunAsync();
    }
}
