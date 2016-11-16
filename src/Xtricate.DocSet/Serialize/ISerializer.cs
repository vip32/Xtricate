namespace Xtricate.DocSet
{
    public interface ISerializer
    {
        string ToJson(object value);

        T FromJson<T>(string value);
    }
}