using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Buffers.Internal
{
    public interface IMessageComponent
    {
        void WriteToBuffer(byte[] messageBuffer, ref int offset);
        int Length { get; }
    }
}
