namespace HttpsRichardy.Federation.Sdk.Clients;

public sealed class PermissionsClient(HttpClient httpClient) : IPermissionsClient
{
    public async Task<Result<PermissionDetails>> CreatePermissionAsync(PermissionCreationScheme permission, CancellationToken cancellation = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/permissions", permission, cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result<PermissionDetails>.Failure(SdkErrors.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<PermissionDetails>.Failure(error)
                : Result<PermissionDetails>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<PermissionDetails>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<PermissionDetails>.Success(result)
            : Result<PermissionDetails>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result<PermissionDetails>> UpdatePermissionAsync(PermissionUpdateScheme permission, CancellationToken cancellation = default)
    {
        var response = await httpClient.PutAsJsonAsync($"api/v1/permissions/{permission.PermissionId}", permission, cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result<PermissionDetails>.Failure(SdkErrors.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<PermissionDetails>.Failure(error)
                : Result<PermissionDetails>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<PermissionDetails>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<PermissionDetails>.Success(result)
            : Result<PermissionDetails>.Failure(SdkErrors.DeserializationFailure);
    }

    public async Task<Result> DeletePermissionAsync(string permissionId, CancellationToken cancellation = default)
    {
        var response = await httpClient.DeleteAsync($"api/v1/permissions/{permissionId}", cancellation);
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

    public async Task<Result<Pagination<PermissionDetails>>> GetPermissionsAsync(PermissionsFetchParameters parameters, CancellationToken cancellation = default)
    {
        string queryString = QueryParametersParser.ToQueryString(parameters);
        string url = $"api/v1/permissions?{queryString}";

        var response = await httpClient.GetAsync(url, cancellation);
        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return Result<Pagination<PermissionDetails>>.Failure(SdkErrors.Unauthorized);
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<Error>(
                options: JsonSerialization.SerializerOptions,
                cancellationToken: cancellation
            );

            return error is not null
                ? Result<Pagination<PermissionDetails>>.Failure(error)
                : Result<Pagination<PermissionDetails>>.Failure(SdkErrors.DeserializationFailure);
        }

        var result = await response.Content.ReadFromJsonAsync<Pagination<PermissionDetails>>(
            options: JsonSerialization.SerializerOptions,
            cancellationToken: cancellation
        );

        return result is not null
            ? Result<Pagination<PermissionDetails>>.Success(result)
            : Result<Pagination<PermissionDetails>>.Failure(SdkErrors.DeserializationFailure);
    }
}
