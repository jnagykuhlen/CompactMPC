using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public interface IObliviousTransfer
    {
        Task SendAsync(Stream stream, BitQuadruple options);
        Task<Bit> ReceiveAsync(Stream stream, int selectionIndex);
    }
}
