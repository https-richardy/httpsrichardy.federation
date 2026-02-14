namespace HttpsRichardy.Federation.Common.Configuration;

public interface ISettings
{
    public Administration Administration { get; }
    public DatabaseSettings Database { get; }
}