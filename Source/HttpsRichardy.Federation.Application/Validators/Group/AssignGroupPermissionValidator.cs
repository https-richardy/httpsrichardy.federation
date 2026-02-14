namespace HttpsRichardy.Federation.Application.Validators.Group;

public sealed class AssignGroupPermissionValidator : AbstractValidator<AssignGroupPermissionScheme>
{
    public AssignGroupPermissionValidator()
    {
        RuleFor(request => request.PermissionName)
            .NotEmpty()
            .WithMessage("Permission name must not be empty.")
            .MinimumLength(3)
            .WithMessage("Permission name must be at least 3 characters long.")
            .MaximumLength(200)
            .WithMessage("Permission name must be at most 200 characters long.");
    }
}
