namespace HttpsRichardy.Federation.Application.Payloads.Connect;

public sealed record JsonWebKeySetScheme
{
    public IEnumerable<JsonWebKeyScheme> Keys { get; set; } = [];
}