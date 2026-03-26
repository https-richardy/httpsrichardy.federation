namespace HttpsRichardy.Federation.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class HttpPipelineExtension
{
    public static void UseHttpPipeline(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseCors();

        app.UseRealmMiddleware();
        app.UseAuthentication();
        app.UsePrincipalMiddleware();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });
    }
}
