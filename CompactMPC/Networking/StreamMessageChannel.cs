using System.IO;
using System.Threading.Tasks;
using CompactMPC.Buffers;

namespace CompactMPC.Networking
{
    public class StreamMessageChannel : IMessageChannel
    {
        private readonly Stream _stream;

        public StreamMessageChannel(Stream stream)
        {
            _stream = stream;
        }

        public async Task<Message> ReadMessageAsync()
        {
            int numberOfBytes = await _stream.ReadInt32Async();
            return new Message(await _stream.ReadAsync(numberOfBytes));
        }

        public async Task WriteMessageAsync(Message message)
        {
            await _stream.WriteInt32Async(message.Length);
            await _stream.WriteAsync(message.ToBuffer());
        }
    }
}
