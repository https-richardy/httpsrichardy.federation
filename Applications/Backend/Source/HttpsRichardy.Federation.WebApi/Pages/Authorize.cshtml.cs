namespace HttpsRichardy.Federation.WebApi.Pages;

public sealed class AuthorizePage : PageModel
{
    private readonly IDispatcher _dispatcher;
    private readonly IUserCollection _userCollection;

    private readonly ITokenCollection _tokenCollection;
    private readonly IRealmCollection _realmCollection;
    private readonly IClientCollection _clientCollection;
    private readonly IRealmProvider _realmProvider;

    #region constructors
    public AuthorizePage(
        IDispatcher dispatcher,
        IUserCollection userCollection,
        IRealmProvider realmProvider,
        IRealmCollection realmCollection,
        ITokenCollection tokenCollection,
        IClientCollection clientCollection)
    {
        _dispatcher = dispatcher;
        _userCollection = userCollection;
        _realmCollection = realmCollection;
        _realmProvider = realmProvider;
        _tokenCollection = tokenCollection;
        _clientCollection = clientCollection;
    }
    #endregion

    [property: BindProperty(SupportsGet = true)]
    public AuthorizationParameters Parameters { get; set; } = new();

    [property: BindProperty]
    public AuthenticationCredentials Credentials { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var filters = ClientFilters.WithSpecifications()
            .WithClientId(Parameters.ClientId)
            .Build();

        var clients = await _clientCollection.GetClientsAsync(filters);
        var client = clients.FirstOrDefault();

        if (client is null)
        {
            ModelState.AddModelError(
                key: ClientErrors.ClientDoesNotExist.Code,
                errorMessage: ClientErrors.ClientDoesNotExist.Description
            );

            return Page();
        }

        var realmFilters = RealmFilters.WithSpecifications()
            .WithIdentifier(client.RealmId)
            .Build();

        var realms = await _realmCollection.GetRealmsAsync(realmFilters);
        var realm = realms.First();

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
            ModelState.AddModelError(
                key: result.Error.Code,
                errorMessage: result.Error.Description
            );

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
            ModelState.AddModelError(
                key: AuthenticationErrors.UserNotFound.Code,
                errorMessage: AuthenticationErrors.UserNotFound.Description
            );

            return Page();
        }

        var code = Guid.NewGuid().ToString("N").ToUpperInvariant();
        var metadata = new Dictionary<string, string>
        {
            { "client.id", Parameters.ClientId ?? string.Empty },
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
