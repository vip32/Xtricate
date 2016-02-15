using ServiceStack.Text;

namespace Xtricate.DocSet.Extras
{
    public class ServiceStackTextSerializer : ISerializer
    {
        public string ToJson(object value)
        {
            if (value == null) return null;
            return TypeSerializer.SerializeToString(value);
        }

        public T FromJson<T>(string value)
        {
            if (value == null) return default(T);
            return TypeSerializer.DeserializeFromString<T>(value);
        }
    }
}