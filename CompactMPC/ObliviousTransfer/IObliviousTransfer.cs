using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public interface IObliviousTransfer
    {
        Task SendAsync(Stream stream, BitQuadrupleArray options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(Stream stream, QuadrupleIndexArray selectionIndices, int numberOfInvocations);
    }
}
