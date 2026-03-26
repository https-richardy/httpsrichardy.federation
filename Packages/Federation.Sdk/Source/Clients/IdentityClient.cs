namespace HttpsRichardy.Federation.Sdk.Clients;

public sealed class IdentityClient(HttpClient httpClient) : IIdentityClient
{
    public async Task<Result<AuthenticationResult>> AuthenticateAsync(
        AuthenticationCredentials credentials, CancellationToken cancellation = default
    )
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/identity/authenticate", credentials, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<AuthenticationResult>.Failure(error)
                : Result<AuthenticationResult>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<AuthenticationResult>.Success(result)
            : Result<AuthenticationResult>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result<UserDetails>> CreateIdentityAsync(IdentityEnrollmentCredentials credentials, CancellationToken cancellation = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/identity", credentials, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<UserDetails>.Failure(error)
                : Result<UserDetails>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<UserDetails>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<UserDetails>.Success(result)
            : Result<UserDetails>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result> InvalidateSessionAsync(SessionInvalidation session, CancellationToken cancellation = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/identity/invalidate-session", session, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result.Failure(error)
                : Result.Failure(SdkErrors.DeserializationFailure);
        }

        return Result.Success();
    }
}