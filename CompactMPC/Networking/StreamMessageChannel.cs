using System.IO;
using System.Threading.Tasks;
using CompactMPC.Buffers;

namespace CompactMPC.Networking;

public class StreamMessageChannel(Stream stream) : IMessageChannel
{
    public async Task<Message> ReadMessageAsync()
    {
        var numberOfBytes = await stream.ReadInt32Async();
        return new Message(await stream.ReadAsync(numberOfBytes));
    }

    public async Task WriteMessageAsync(Message message)
    {
        await stream.WriteInt32Async(message.Length);
        await stream.WriteAsync(message.ToBuffer());
    }
}
