namespace HttpsRichardy.Federation.Application.Validators.Client;

public sealed class ClientUpdateSchemeValidator : AbstractValidator<ClientUpdateScheme>
{
    public ClientUpdateSchemeValidator()
    {
        RuleFor(client => client.Name)
            .NotEmpty()
            .WithMessage("client name must not be empty.")
            .MaximumLength(100)
            .WithMessage("client name must not exceed 100 characters.")
            .MinimumLength(3)
            .WithMessage("client name must be at least 3 characters long.");

        RuleFor(client => client.Flows)
            .NotEmpty()
            .WithMessage("client must have at least one flow.");

        RuleForEach(client => client.Flows)
            .IsInEnum()
            .WithMessage("client flow must be a valid grant type.");

        When(client => client.RedirectUris is not null && client.RedirectUris.Any(), () =>
        {
            RuleForEach(client => client.RedirectUris)
                .Must(uri => !string.IsNullOrWhiteSpace(uri) && Uri.TryCreate(uri, UriKind.Absolute, out var parsed) &&
                     (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps))
                .WithMessage("redirect uri must be a valid url.");
        });
    }
}
