namespace HttpsRichardy.Federation.Application.Validators.Permission;

public sealed class PermissionUpdateValidator : AbstractValidator<PermissionUpdateScheme>
{
    public PermissionUpdateValidator()
    {
        RuleFor(permission => permission.Name)
            .NotEmpty()
            .WithMessage("Permission name must not be empty.")
            .MaximumLength(200)
            .WithMessage("Permission name must be at most 200 characters long.");

        RuleFor(permission => permission.Description)
            .MaximumLength(500)
            .WithMessage("Permission description must be at most 500 characters long.");
    }
}
