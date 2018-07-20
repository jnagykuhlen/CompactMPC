using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IObliviousTransferStream
    {
        Task SendAsync(int instanceId, BitQuadruple options);
        Task<Bit> ReceiveAsync(int instanceId, int selectionIndex);
    }
}
