namespace HttpsRichardy.Federation.WebApi.Constants;

public static class Authentication
{
    private static readonly JsonSerializerOptions _serializer = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static readonly JwtBearerEvents Events = new()
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            if (context.Exception is SecurityTokenExpiredException expiredException)
                return context.Response.WriteAsync(JsonSerializer.Serialize(AuthenticationErrors.TokenExpired, _serializer));

            if (context.Exception is SecurityTokenInvalidSignatureException)
                return context.Response.WriteAsync(JsonSerializer.Serialize(AuthenticationErrors.InvalidSignature, _serializer));

            return context.Response.WriteAsync(JsonSerializer.Serialize(AuthenticationErrors.InvalidTokenFormat, _serializer));
        },

        OnChallenge = context =>
        {
            context.HandleResponse();

            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            return context.Response.WriteAsync(JsonSerializer.Serialize(AuthenticationErrors.Unauthenticated, _serializer));
        }
    };
}