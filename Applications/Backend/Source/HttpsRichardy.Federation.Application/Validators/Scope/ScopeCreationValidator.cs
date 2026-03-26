namespace HttpsRichardy.Federation.Application.Validators.Scope;

public sealed class ScopeCreationValidator : AbstractValidator<ScopeCreationScheme>
{
    public ScopeCreationValidator()
    {
        RuleFor(scope => scope.Name)
            .NotEmpty()
            .WithMessage("Scope name must not be empty.")
            .MaximumLength(200)
            .WithMessage("Scope name must be at most 200 characters long.");

        RuleFor(scope => scope.Description)
            .MaximumLength(500)
            .WithMessage("Scope description must be at most 500 characters long.");
    }
}
