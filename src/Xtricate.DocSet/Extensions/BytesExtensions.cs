using System.IO;
using System.IO.Compression;

namespace Xtricate.DocSet
{
    public static class BytesExtensions
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
    }
}
