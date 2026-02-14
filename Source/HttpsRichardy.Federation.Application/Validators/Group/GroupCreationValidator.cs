namespace HttpsRichardy.Federation.Application.Validators.Group;

public sealed class GroupCreationValidator : AbstractValidator<GroupCreationScheme>
{
    public GroupCreationValidator()
    {
        RuleFor(group => group.Name)
            .NotEmpty()
            .WithMessage("Group name must not be empty.")
            .MinimumLength(3)
            .WithMessage("Group name must be at least 3 characters long.")
            .MaximumLength(100)
            .WithMessage("Group name must be at most 100 characters long.");
    }
}