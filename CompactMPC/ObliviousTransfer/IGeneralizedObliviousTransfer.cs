using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    public interface IGeneralizedObliviousTransfer
    {
        Task SendAsync(Stream stream, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(Stream stream, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
