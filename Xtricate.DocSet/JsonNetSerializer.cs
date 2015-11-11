using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Xtricate.DocSet
{
    public class JsonNetSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
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
            return JsonConvert.DeserializeObject<T>(value, _settings);
        }
    }
}