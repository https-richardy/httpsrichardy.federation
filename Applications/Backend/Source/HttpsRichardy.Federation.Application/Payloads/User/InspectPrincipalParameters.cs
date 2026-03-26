namespace HttpsRichardy.Federation.Application.Payloads.User;

public sealed record InspectPrincipalParameters :
    IDispatchable<Result<PrincipalDetailsScheme>>;