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

        OnTokenValidated = context =>
        {
            var request = context.HttpContext.Request;
            if (context.SecurityToken is not Microsoft.IdentityModel.JsonWebTokens.JsonWebToken token)
            {
                context.HttpContext.Items["authentication.error"] = AuthenticationErrors.InvalidTokenFormat;
                context.Fail("The token format is invalid or the token is malformed.");

                return Task.CompletedTask;
            }

            var expectedIssuer = $"{request.Scheme}://{request.Host}".TrimEnd('/');
            var actualIssuer = token.Issuer?.TrimEnd('/');

            if (!string.Equals(actualIssuer, expectedIssuer, StringComparison.OrdinalIgnoreCase))
            {
                context.HttpContext.Items["authentication.error"] = AuthenticationErrors.InvalidIssuer;
                context.Fail("The token issuer is invalid.");

                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        },

        OnChallenge = context =>
        {
            context.HandleResponse();

            if (context.Response.HasStarted)
                return Task.CompletedTask;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var error = context.HttpContext.Items["authentication.error"] as Error
                        ?? AuthenticationErrors.Unauthenticated;

            return context.Response.WriteAsync(JsonSerializer.Serialize(error, _serializer));
        }
    };
}

