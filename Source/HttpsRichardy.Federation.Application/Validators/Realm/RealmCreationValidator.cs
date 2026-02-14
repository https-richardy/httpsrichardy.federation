namespace HttpsRichardy.Federation.Application.Validators.Realm;

public sealed class RealmCreationValidator : AbstractValidator<RealmCreationScheme>
{
    public RealmCreationValidator()
    {
        RuleFor(realm => realm.Name)
            .NotEmpty()
            .WithMessage("realm name must not be empty.")
            .MinimumLength(3)
            .WithMessage("realm name must be at least 3 characters long.")
            .MaximumLength(100)
            .WithMessage("realm name must be at most 100 characters long.");

        RuleFor(realm => realm.Description)
            .MaximumLength(500)
            .WithMessage("realm description must be at most 500 characters long.")
            .When(realm => !string.IsNullOrEmpty(realm.Description));
    }
}
