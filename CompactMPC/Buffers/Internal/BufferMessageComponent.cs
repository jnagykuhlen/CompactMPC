using System;

namespace CompactMPC.Buffers.Internal
{
    public class BufferMessageComponent : IMessageComponent
    {
        private readonly byte[] _buffer;

        public BufferMessageComponent(byte[] buffer)
        {
            _buffer = buffer;
        }

        public void WriteToBuffer(byte[] messageBuffer, ref int offset)
        {
            Buffer.BlockCopy(_buffer, 0, messageBuffer, offset, _buffer.Length);
            offset += _buffer.Length;
        }

        public static byte[] ReadFromBuffer(byte[] messageBuffer, ref int offset, int length)
        {
            byte[] buffer = new byte[length];
            Buffer.BlockCopy(messageBuffer, offset, buffer, 0, length);
            offset += length;
            return buffer;
        }

        public int Length
        {
            get
            {
                return _buffer.Length;
            }
        }
    }
}
