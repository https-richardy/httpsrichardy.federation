namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class RevokeAudienceFromClientHandler(IClientCollection clientCollection) :
    IDispatchHandler<RevokeClientAudienceScheme, Result<IReadOnlyCollection<string>>>
{
    public async Task<Result<IReadOnlyCollection<string>>> HandleAsync(
        RevokeClientAudienceScheme parameters, CancellationToken cancellation = default)
    {
        var filters = ClientFilters.WithSpecifications()
            .WithIdentifier(parameters.Id)
            .Build();

        var clients = await clientCollection.GetClientsAsync(filters, cancellation);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            return Result<IReadOnlyCollection<string>>.Failure(ClientErrors.ClientDoesNotExist);
        }

        var audienceToRemove = client.Audiences.FirstOrDefault(current => current.Value == parameters.Audience);
        if (audienceToRemove is null)
        {
            return Result<IReadOnlyCollection<string>>.Failure(ClientErrors.AudienceNotAssigned);
        }

        client.Audiences.Remove(audienceToRemove);

        await clientCollection.UpdateAsync(client, cancellation);

        return Result<IReadOnlyCollection<string>>.Success([.. client.Audiences.Select(current => current.Value)]);
    }
}
