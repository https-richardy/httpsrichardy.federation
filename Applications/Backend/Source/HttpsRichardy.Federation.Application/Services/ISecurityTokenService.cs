namespace HttpsRichardy.Federation.Application.Services;

public interface ISecurityTokenService
{
    public Task<Result<SecurityToken>> GenerateAccessTokenAsync(
        User user,
        CancellationToken cancellation = default
    );

    public Task<Result<SecurityToken>> GenerateAccessTokenAsync(
        Realm realm,
        CancellationToken cancellation = default
    );

    public Task<Result<SecurityToken>> GenerateRefreshTokenAsync(
        User user,
        CancellationToken cancellation = default
    );

    public Task<Result> ValidateAccessTokenAsync(
        SecurityToken token,
        CancellationToken cancellation = default
    );

    public Task<Result> ValidateRefreshTokenAsync(
        SecurityToken token,
        CancellationToken cancellation = default
    );

    public Task<Result> RevokeRefreshTokenAsync(
        SecurityToken token,
        CancellationToken cancellation = default
    );
}