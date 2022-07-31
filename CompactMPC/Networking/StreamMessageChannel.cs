using System;
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
            int numberOfBytes = BitConverter.ToInt32(await _stream.ReadAsync(4), 0);
            return new Message(await _stream.ReadAsync(numberOfBytes));
        }

        public async Task WriteMessageAsync(Message message)
        {
            await _stream.WriteAsync(BitConverter.GetBytes(message.Length));
            await _stream.WriteAsync(message.ToBuffer());
        }
    }
}
