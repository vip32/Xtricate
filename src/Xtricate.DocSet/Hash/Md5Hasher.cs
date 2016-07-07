using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Xtricate.DocSet
{
    public class Md5Hasher : IHasher
    {
        public string Compute(object value)
        {
            if (value == null) return null;

            using (var provider = new MD5CryptoServiceProvider())
            {
                var hash = provider.ComputeHash(ToBytes(value));
                var hex = BitConverter.ToString(hash);
                return hex.Replace("-", "");
            }
        }

        private static byte[] ToBytes<T>(T obj) where T : class
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