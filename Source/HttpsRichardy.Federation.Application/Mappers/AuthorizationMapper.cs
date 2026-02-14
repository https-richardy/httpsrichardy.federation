namespace HttpsRichardy.Federation.Application.Mappers;

public static class AuthorizationMapper
{
    public static AuthorizationScheme AsReponse(this AuthorizationParameters parameters) => new()
    {
        ClientId = parameters.ClientId,
        RedirectUri = parameters.RedirectUri,
        State = parameters.State,
        CodeChallenge = parameters.CodeChallenge,
        CodeChallengeMethod = parameters.CodeChallengeMethod,
    };
}
