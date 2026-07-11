using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC;

public static class StreamExtensions
{
    public static async Task<byte[]> ReadAsync(this Stream stream, int count)
    {
        var buffer = new byte[count];
        var offset = 0;

        while (offset < count)
        {
            var readBytes = await stream.ReadAsync(buffer, offset, count - offset);
            offset += readBytes;
        }

        return buffer;
    }

    public static async Task<int> ReadInt32Async(this Stream stream) =>
        BitConverter.ToInt32(await stream.ReadAsync(4));

    public static async Task WriteInt32Async(this Stream stream, int value) =>
        await stream.WriteAsync(BitConverter.GetBytes(value));

    public static async Task<string> ReadStringAsync(this Stream stream) {
        var length = await stream.ReadInt32Async();
        return Encoding.UTF8.GetString(await stream.ReadAsync(length));
    }

    public static async Task WriteStringAsync(this Stream stream, string value) {
        var bytes = Encoding.UTF8.GetBytes(value);
        await stream.WriteInt32Async(bytes.Length);
        await stream.WriteAsync(bytes);
    }

    public static async Task<Guid> ReadGuidAsync(this Stream stream) =>
        new(await stream.ReadAsync(16));

    public static async Task WriteGuidAsync(this Stream stream, Guid value) =>
        await stream.WriteAsync(value.ToByteArray());
}