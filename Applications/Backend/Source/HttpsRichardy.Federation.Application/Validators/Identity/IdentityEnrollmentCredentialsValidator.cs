namespace HttpsRichardy.Federation.Application.Validators.Identity;

public sealed class IdentityEnrollmentCredentialsValidator :
    AbstractValidator<IdentityEnrollmentCredentials>
{
    public IdentityEnrollmentCredentialsValidator()
    {
        RuleFor(credential => credential.Username)
            .NotEmpty()
            .WithMessage("Username must not be empty.")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(100)
            .WithMessage("Username must be at most 100 characters long.")
            .Matches(@"^[\w\.\-\@]+$")
            .WithMessage("Username contains invalid characters.");

        RuleFor(credential => credential.Password)
            .NotEmpty()
            .WithMessage("Password must not be empty.")
            .MaximumLength(200)
            .WithMessage("Password must be at most 200 characters long.")
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("Password cannot be whitespace only.");
    }
}