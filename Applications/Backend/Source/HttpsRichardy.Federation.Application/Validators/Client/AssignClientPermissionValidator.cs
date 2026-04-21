namespace HttpsRichardy.Federation.Application.Validators.Client;

public sealed class AssignClientPermissionValidator : AbstractValidator<AssignClientPermissionScheme>
{
    public AssignClientPermissionValidator()
    {
        RuleFor(request => request.PermissionName)
            .NotEmpty()
            .WithMessage("permission name must not be empty.")
            .MinimumLength(3)
            .WithMessage("permission name must be at least 3 characters long.")
            .MaximumLength(200)
            .WithMessage("permission name must be at most 200 characters long.");
    }
}
