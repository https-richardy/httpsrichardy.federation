namespace HttpsRichardy.Federation.WebApi.Pages;

public sealed class AuthorizePage : PageModel
{
    private readonly IDispatcher _dispatcher;
    private readonly IUserCollection _userCollection;

    private readonly ITokenCollection _tokenCollection;
    private readonly IRealmCollection _realmCollection;
    private readonly IRealmProvider _realmProvider;

    #region constructors
    public AuthorizePage(
        IDispatcher dispatcher,
        IUserCollection userCollection,
        IRealmProvider realmProvider,
        IRealmCollection realmCollection,
        ITokenCollection tokenCollection)
    {
        _dispatcher = dispatcher;
        _userCollection = userCollection;
        _realmCollection = realmCollection;
        _realmProvider = realmProvider;
        _tokenCollection = tokenCollection;
    }
    #endregion

    [property: BindProperty(SupportsGet = true)]
    public AuthorizationParameters Parameters { get; set; } = new();

    [property: BindProperty]
    public AuthenticationCredentials Credentials { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var filters = RealmFilters.WithSpecifications()
            .WithClientId(Parameters.ClientId)
            .Build();

        var realms = await _realmCollection.GetRealmsAsync(filters);
        var realm = realms.FirstOrDefault();

        if (realm is null)
        {
            ModelState.AddModelError(
                key: RealmErrors.RealmDoesNotExist.Code,
                errorMessage: RealmErrors.RealmDoesNotExist.Description
            );

            return Page();
        }

        _realmProvider.SetRealm(realm);

        var result = await _dispatcher.DispatchAsync(Parameters);
        if (result.IsFailure)
        {
            ModelState.AddModelError(
                key: result.Error.Code,
                errorMessage: result.Error.Description
            );

            return Page();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await _dispatcher.DispatchAsync(Credentials);
        if (result.IsFailure)
        {
            ModelState.AddModelError(result.Error.Code, result.Error.Description);
            return Page();
        }

        var realm = _realmProvider.GetCurrentRealm();
        var filters = UserFilters.WithSpecifications()
            .WithUsername(Credentials.Username)
            .WithRealmId(realm.Id)
            .Build();

        var users = await _userCollection.GetUsersAsync(filters);
        var user = users.FirstOrDefault();

        if (user is null)
        {
            ModelState.AddModelError(AuthenticationErrors.UserNotFound.Code, AuthenticationErrors.UserNotFound.Description);
            return Page();
        }

        var code = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var metadata = new Dictionary<string, string>
        {
            { "code.challenge", Parameters.CodeChallenge ?? string.Empty },
            { "code.challenge.method", Parameters.CodeChallengeMethod ?? string.Empty }
        };

        var token = new Domain.Aggregates.SecurityToken
        {
            UserId = user.Id,
            RealmId = realm.Id,
            Metadata = metadata,
            Value = code,
            Type = TokenType.AuthorizationCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
        };

        await _tokenCollection.InsertAsync(token);

        return Redirect($"{Parameters.RedirectUri}?code={code}&state={Parameters.State}");
    }
}
