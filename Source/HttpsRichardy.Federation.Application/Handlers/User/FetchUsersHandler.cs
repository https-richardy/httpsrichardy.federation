namespace HttpsRichardy.Federation.Application.Handlers.User;

public sealed class FetchUsersHandler(IUserCollection collection) :
    IDispatchHandler<UsersFetchParameters, Result<Pagination<UserDetailsScheme>>>
{
    public async Task<Result<Pagination<UserDetailsScheme>>> HandleAsync(
        UsersFetchParameters parameters, CancellationToken cancellation = default)
    {
        var filters = UserMapper.AsFilters(parameters);

        var users = await collection.GetUsersAsync(filters, cancellation);
        var totalUsers = await collection.CountAsync(filters, cancellation);

        var pagination = new Pagination<UserDetailsScheme>
        {
            Items = [.. users.Select(user => UserMapper.AsResponse(user))],
            Total = (int) totalUsers,
            PageNumber = parameters.Pagination?.PageNumber ?? 1,
            PageSize = parameters.Pagination?.PageSize ?? 20,
        };

        return Result<Pagination<UserDetailsScheme>>.Success(pagination);
    }
}
