using Jil;
using Newtonsoft.Json;

namespace Xtricate.DocSet
{
    public class JilSerializer : ISerializer
    {
        public string ToJson(object value)
        {
            if (value == null) return null;
            return JSON.SerializeDynamic(value, new Options(excludeNulls: true));

        }

        public T FromJson<T>(string value)
        {
            if (value == null) return default(T);
            return JSON.DeserializeDynamic(value);
        }
    }
}