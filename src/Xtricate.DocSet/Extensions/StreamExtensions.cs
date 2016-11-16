using System.IO;

namespace Xtricate.DocSet
{
    public static class StreamExtensions
    {
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