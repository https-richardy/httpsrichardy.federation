namespace HttpsRichardy.Federation.Domain.Concepts;

public sealed record Audience(string Value) :
    IValueObject<Audience>
{
    public string Value { get; init; } = Value;
}
