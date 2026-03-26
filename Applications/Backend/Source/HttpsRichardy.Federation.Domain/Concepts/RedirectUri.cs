namespace HttpsRichardy.Federation.Domain.Concepts;

public sealed record RedirectUri(string Address) :
    IValueObject<RedirectUri>
{
    public string Address { get; init; } = Address;
}
