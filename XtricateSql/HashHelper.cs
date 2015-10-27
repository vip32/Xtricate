using System;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace XtricateSql
{
    public static class HashHelper
    {
        public static string ComputeHash(object instance)
        {
            if (instance == null) return null;
            var bytes = BsonByteSerialize(instance);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                var hash = md5.ComputeHash(bytes);
                var hex = BitConverter.ToString(hash);
                return hex.Replace("-", "");
            }
        }

        public static byte[] BsonByteSerialize<T>(T obj) where T : class
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