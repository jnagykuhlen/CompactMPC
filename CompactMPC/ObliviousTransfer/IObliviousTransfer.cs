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
        Task SendAsync(Stream stream, BitQuadruple[] options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfInvocations);
    }
}
