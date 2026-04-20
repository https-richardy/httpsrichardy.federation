namespace HttpsRichardy.Federation.Application.Handlers.Client;

public sealed class AssignAudienceClientHandler(IClientCollection clientCollection) :
    IDispatchHandler<AssignClientAudienceScheme, Result<IReadOnlyCollection<string>>>
{
    public async Task<Result<IReadOnlyCollection<string>>> HandleAsync(
        AssignClientAudienceScheme parameters, CancellationToken cancellation = default)
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

        var audience = new Audience(parameters.Value);
        if (client.Audiences.Any(current => current.Value == audience.Value))
        {
            return Result<IReadOnlyCollection<string>>.Failure(ClientErrors.ClientAlreadyHasAudience);
        }

        client.Audiences.Add(audience);

        await clientCollection.UpdateAsync(client, cancellation);

        return Result<IReadOnlyCollection<string>>.Success([.. client.Audiences.Select(current => current.Value)]);
    }
}
