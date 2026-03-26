namespace HttpsRichardy.Federation.Sdk.Contracts.Clients;

public interface IIdentityClient
{
    public Task<Result<AuthenticationResult>> AuthenticateAsync(
        AuthenticationCredentials credentials,
        CancellationToken cancellation = default
    );

    public Task<Result<UserDetails>> CreateIdentityAsync(
        IdentityEnrollmentCredentials credentials,
        CancellationToken cancellation = default
    );

    public Task<Result> InvalidateSessionAsync(
        SessionInvalidation session,
        CancellationToken cancellation = default
    );
}