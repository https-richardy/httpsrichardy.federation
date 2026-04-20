namespace HttpsRichardy.Federation.Application.Validators.Client;

public sealed class AssignClientAudienceValidator : AbstractValidator<AssignClientAudienceScheme>
{
    public AssignClientAudienceValidator()
    {
        RuleFor(scheme => scheme.Value)
            .NotEmpty()
            .WithMessage("audience value is required.")
            .MaximumLength(500)
            .WithMessage("audience value cannot exceed 500 characters.");
    }
}
