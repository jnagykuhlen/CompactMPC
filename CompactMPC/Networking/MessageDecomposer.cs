using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Networking.Internal;

namespace CompactMPC.Networking
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
            byte[] buffer = BufferMessageComponent.ReadFromBuffer(_messageBuffer, _offset, length);
            _offset += length;
            return buffer;
        }

        public int ReadInt()
        {
            int value = IntMessageComponent.ReadFromBuffer(_messageBuffer, _offset);
            _offset += sizeof(int);
            return value;
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
