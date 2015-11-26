using Jil;

namespace Xtricate.DocSet
{
    public class JilSerializer : ISerializer
    {
        public string ToJson(object value)
        {
            if (value == null) return null;
            return JSON.Serialize /*Dynamic*/(value,
                new Options(excludeNulls: true, serializationNameFormat: SerializationNameFormat.CamelCase));
        }

        public T FromJson<T>(string value)
        {
            if (value == null) return default(T);
            return JSON.Deserialize /*Dynamic*/<T>(value,
                new Options(excludeNulls: true, serializationNameFormat: SerializationNameFormat.CamelCase));
        }
    }
}