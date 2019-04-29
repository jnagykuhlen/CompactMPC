using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Buffers.Internal;

namespace CompactMPC.Buffers
{
    public class MessageDecomposer
    {
        private byte[] _messageBuffer;
        private int _offset;

        public MessageDecomposer(byte[] messageBuffer)
        {
            _messageBuffer = messageBuffer;
            _offset = 0;
        }

        public byte[] ReadBuffer(int length)
        {
            return BufferMessageComponent.ReadFromBuffer(_messageBuffer, ref _offset, length);
        }

        public int ReadInt()
        {
            return IntMessageComponent.ReadFromBuffer(_messageBuffer, ref _offset);
        }

        public int Length
        {
            get
            {
                return _messageBuffer.Length;
            }
        }
    }
}
