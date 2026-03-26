namespace HttpsRichardy.Federation.Application.Handlers.Realm;

public sealed class FetchRealmsHandler(IRealmCollection collection) :
    IDispatchHandler<RealmFetchParameters, Result<Pagination<RealmDetailsScheme>>>
{
    public async Task<Result<Pagination<RealmDetailsScheme>>> HandleAsync(
        RealmFetchParameters parameters, CancellationToken cancellation = default)
    {
        var filters = RealmMapper.AsFilters(parameters);

        var realms = await collection.GetRealmsAsync(filters, cancellation);
        var totalRealms = await collection.CountAsync(filters, cancellation);

        var pagination = new Pagination<RealmDetailsScheme>
        {
            Items = [.. realms.Select(realm => RealmMapper.AsResponse(realm))],
            Total = (int) totalRealms,
            PageNumber = parameters.Pagination?.PageNumber ?? 1,
            PageSize = parameters.Pagination?.PageSize ?? 20
        };

        return Result<Pagination<RealmDetailsScheme>>.Success(pagination);
    }
}
