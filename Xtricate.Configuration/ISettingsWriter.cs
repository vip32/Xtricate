namespace Xtricate.Configuration
{
    public interface ISettingsWriter : ISettings
    {
        void Set<T>(string key, T value);
    }
}