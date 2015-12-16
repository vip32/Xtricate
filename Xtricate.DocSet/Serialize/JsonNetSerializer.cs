using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xtricate.DocSet
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //TypeNameHandling = TypeNameHandling.All,
            //ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[]
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter()
            }
        };

        public string ToJson(object value)
        {
            if (value == null) return null;
            return JsonConvert.SerializeObject(value, _settings);
        }

        public T FromJson<T>(string value)
        {
            if (value == null) return default(T);
            try
            {
                return JsonConvert.DeserializeObject<T>(value, _settings);
            }
            catch (JsonException ex)
            {
                throw new JsonException(string.Format(
                    "Json deserialization failed. The Json data (value) does not conform to the target entity '{0}' format. See innerexception for details.",
                    typeof(T).Name), ex);
            }
        }
    }
}