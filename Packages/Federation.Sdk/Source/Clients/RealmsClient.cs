namespace HttpsRichardy.Federation.Sdk.Clients;

public sealed class RealmsClient(HttpClient httpClient) : IRealmsClient
{
    public async Task<Result<Pagination<RealmDetails>>> GetRealmsAsync(
        RealmFetchParameters parameters, CancellationToken cancellation = default)
    {
        string queryString = QueryParametersParser.ToQueryString(parameters);
        string url = $"api/v1/realms?{queryString}";

        var response = await httpClient.GetAsync(url, cancellation);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<Pagination<RealmDetails>>.Failure(error)
                : Result<Pagination<RealmDetails>>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<Pagination<RealmDetails>>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<Pagination<RealmDetails>>.Success(result)
            : Result<Pagination<RealmDetails>>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result<RealmDetails>> CreateRealmAsync(RealmCreationScheme realm, CancellationToken cancellation = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/realms", realm, cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result<RealmDetails>.Failure(SdkErrors.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<RealmDetails>.Failure(error)
                : Result<RealmDetails>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<RealmDetails>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<RealmDetails>.Success(result)
            : Result<RealmDetails>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result<RealmDetails>> UpdateRealmAsync(RealmUpdateScheme realm, CancellationToken cancellation = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/v1/realms/{realm.RealmId}", realm, cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result<RealmDetails>.Failure(SdkErrors.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<RealmDetails>.Failure(error)
                : Result<RealmDetails>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<RealmDetails>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<RealmDetails>.Success(result)
            : Result<RealmDetails>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result> DeleteRealmAsync(string realmId, CancellationToken cancellation = default)
    {
        var response = await httpClient.DeleteAsync($"api/v1/realms/{realmId}", cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result.Failure(SdkErrors.Unauthorized);
        }

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