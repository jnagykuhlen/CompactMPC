using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class StreamMessageChannel : IMessageChannel
    {
        private Stream _stream;

        public StreamMessageChannel(Stream stream)
        {
            _stream = stream;
        }

        public async Task<byte[]> ReadMessageAsync()
        {
            int numberOfBytes = BitConverter.ToInt32(await _stream.ReadAsync(4), 0);
            return await _stream.ReadAsync(numberOfBytes);
        }

        public async Task WriteMessageAsync(byte[] message)
        {
            await _stream.WriteAsync(BitConverter.GetBytes(message.Length));
            await _stream.WriteAsync(message);
        }
    }
}
