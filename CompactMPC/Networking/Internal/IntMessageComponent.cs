using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Networking.Internal
{
    public class IntMessageComponent : IMessageComponent
    {
        private int _value;

        public IntMessageComponent(int value)
        {
            _value = value;
        }

        public void WriteToBuffer(byte[] messageBuffer, int offset)
        {
            messageBuffer[offset + 0] = (byte)(_value >> 0);
            messageBuffer[offset + 1] = (byte)(_value >> 8);
            messageBuffer[offset + 2] = (byte)(_value >> 16);
            messageBuffer[offset + 3] = (byte)(_value >> 24);
        }

        public static int ReadFromBuffer(byte[] messageBuffer, int offset)
        {
            return
                (messageBuffer[offset + 0] <<  0) |
                (messageBuffer[offset + 1] <<  8) |
                (messageBuffer[offset + 2] << 16) |
                (messageBuffer[offset + 3] << 24);
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
