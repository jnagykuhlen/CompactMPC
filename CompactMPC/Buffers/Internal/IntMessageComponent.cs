using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Buffers.Internal
{
    public class IntMessageComponent : IMessageComponent
    {
        private int _value;

        public IntMessageComponent(int value)
        {
            _value = value;
        }

        public void WriteToBuffer(byte[] messageBuffer, ref int offset)
        {
            messageBuffer[offset++] = (byte)(_value >> 0);
            messageBuffer[offset++] = (byte)(_value >> 8);
            messageBuffer[offset++] = (byte)(_value >> 16);
            messageBuffer[offset++] = (byte)(_value >> 24);
        }

        public static int ReadFromBuffer(byte[] messageBuffer, ref int offset)
        {
            return
                (messageBuffer[offset++] <<  0) |
                (messageBuffer[offset++] <<  8) |
                (messageBuffer[offset++] << 16) |
                (messageBuffer[offset++] << 24);
        }

        public int Length
        {
            get
            {
                return sizeof(int);
            }
        }
    }
}
