namespace HttpsRichardy.Federation.Application.Handlers.Scope;

public sealed class ScopeCreationHandler(IScopeCollection scopeCollection, IRealmCollection realmCollection, IRealmProvider realmProvider) :
    IDispatchHandler<ScopeCreationScheme, Result<ScopeDetailsScheme>>
{
    public async Task<Result<ScopeDetailsScheme>> HandleAsync(ScopeCreationScheme parameters, CancellationToken cancellation = default)
    {
        var realm = realmProvider.GetCurrentRealm();
        var filters = ScopeFilters.WithSpecifications()
            .WithName(parameters.Name)
            .Build();

        var scopes = await scopeCollection.GetScopesAsync(filters, cancellation: cancellation);
        var existingScope = scopes.FirstOrDefault();

        if (existingScope is not null)
        {
            return Result<ScopeDetailsScheme>.Failure(ScopeErrors.ScopeAlreadyExists);
        }

        var scope = await scopeCollection.InsertAsync(ScopeMapper.AsScope(parameters, realm), cancellation: cancellation);
        var response = ScopeMapper.AsResponse(scope);

        realm.Scopes.Add(scope);

        await realmCollection.UpdateAsync(realm, cancellation: cancellation);

        return Result<ScopeDetailsScheme>.Success(response);
    }
}