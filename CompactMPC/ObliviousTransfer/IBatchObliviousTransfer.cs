using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public interface IBatchObliviousTransfer
    {
        Task SendAsync(Stream stream, Quadruple<byte[]>[] options, int numberOfMessageBytes, int numberOfInvocations);
        Task<byte[][]> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfMessageBytes, int numberOfInvocations);
    }
}
