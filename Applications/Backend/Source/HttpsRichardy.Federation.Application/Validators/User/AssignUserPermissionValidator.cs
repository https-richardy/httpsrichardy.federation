namespace HttpsRichardy.Federation.Application.Validators.User;

public sealed class AssignUserPermissionValidator : AbstractValidator<AssignUserPermissionScheme>
{
    public AssignUserPermissionValidator()
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
