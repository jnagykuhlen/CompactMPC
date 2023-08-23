using System.IO;
using System.Threading.Tasks;

namespace CompactMPC
{
    public static class StreamHelper
    {
        public static async Task<byte[]> ReadAsync(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;

            while (offset < count)
            {
                int readBytes = await stream.ReadAsync(buffer, offset, count - offset);
                offset += readBytes;
            }

            return buffer;
        }
    }
}
