using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace XtricateSql
{
    public class BsonSerializer : ISerializer
    {
        public string ToJson(object value)
        {
            if (value == null) return null;

            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, value);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public T FromJson<T>(string value)
        {
            if (value == null) return default(T);
            var bytes = Convert.FromBase64String(value);

            var ms = new MemoryStream(bytes);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize<T>(reader);
            }
        }
    }
}