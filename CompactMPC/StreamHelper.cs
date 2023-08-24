using System;
using System.IO;
using System.Text;
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

        public static async Task<int> ReadInt32Async(this Stream stream) {
            return BitConverter.ToInt32(await stream.ReadAsync(4));
        }

        public static async Task WriteInt32Async(this Stream stream, int value) {
            await stream.WriteAsync(BitConverter.GetBytes(value));
        }

        public static async Task<string> ReadStringAsync(this Stream stream) {
            int length = await stream.ReadInt32Async();
            return Encoding.UTF8.GetString(await stream.ReadAsync(length));
        }

        public static async Task WriteStringAsync(this Stream stream, string value) {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            await stream.WriteInt32Async(bytes.Length);
            await stream.WriteAsync(bytes);
        }

        public static async Task<Guid> ReadGuidAsync(this Stream stream) {
            return new Guid(await stream.ReadAsync(16));
        }

        public static async Task WriteGuidAsync(this Stream stream, Guid value) {
            await stream.WriteAsync(value.ToByteArray());
        }
    }
}
