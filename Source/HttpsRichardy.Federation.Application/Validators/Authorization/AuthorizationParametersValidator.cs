namespace HttpsRichardy.Federation.Application.Validators.Authorization;

public sealed class AuthorizationParametersValidator : AbstractValidator<AuthorizationParameters>
{
    public AuthorizationParametersValidator()
    {
        RuleFor(parameters => parameters.ResponseType)
            .NotNull()
            .NotEmpty()
            .Equal("code")
            .WithMessage("response type must be 'code'.");

        RuleFor(parameters => parameters.ClientId)
            .NotNull()
            .NotEmpty();

        RuleFor(parameters => parameters.RedirectUri)
            .NotNull()
            .NotEmpty()
            .Must(uri => uri is not null && Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("redirect uri must be a valid absolute uri.");

        RuleFor(parameters => parameters.CodeChallenge)
            .NotNull()
            .NotEmpty()
            .MinimumLength(43).WithMessage("code challenge must be at least 43 characters for S256.")
            .MaximumLength(128).WithMessage("code challenge must be at most 128 characters for S256.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("code challenge must be base64url encoded.");

        RuleFor(parameters => parameters.CodeChallengeMethod)
            .NotNull()
            .NotEmpty()
            .Must(method => method is not null && (method == SupportedPkceMethods.PkcePlain || method == SupportedPkceMethods.PkceS256))
            .WithMessage("code challenge method must be 'S256' or 'plain'.");
    }
}
