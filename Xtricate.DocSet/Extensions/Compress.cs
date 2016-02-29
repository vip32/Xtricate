using System.IO;
using System.IO.Compression;

namespace Xtricate.DocSet
{
    public static class Extensions
    {
        public static byte[] Compress(this byte[] data)
        {
            if (data == null) return null;
            using (var outStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                using (var srcStream = new MemoryStream(data))
                    srcStream.CopyTo(gzipStream);
                return outStream.ToArray();
            }
        }

        public static void Compress(this Stream srcStream, Stream outStream)
        {
            if (srcStream == null) return;
            using (var gzipStream = new GZipStream(outStream, CompressionMode.Compress))
                srcStream.CopyTo(gzipStream);
        }

        public static byte[] Decompress(this byte[] data)
        {
            if (data == null) return null;
            using (var inStream = new MemoryStream(data))
            using (var gzipStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (var outStream = new MemoryStream())
            {
                gzipStream.CopyTo(outStream);
                return outStream.ToArray();
            }
        }

        public static byte[] ToBytes(this Stream srcStream)
        {
            if (srcStream == null) return null;
            srcStream.Position = 0;
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
