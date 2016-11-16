using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Xtricate.DocSet
{
    public class Sha1Hasher : IHasher
    {
        public string Compute(object value)
        {
            if (value == null) return null;

            using (var provider = new SHA1Managed())
            {
                var hash = provider.ComputeHash(ToBytes(value));
                return Convert.ToBase64String(hash);
            }
        }

        private static byte[] ToBytes<T>(T obj)
            where T : class
        {
            if (obj == null) return null;

            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return ms.ToArray();
        }
    }
}