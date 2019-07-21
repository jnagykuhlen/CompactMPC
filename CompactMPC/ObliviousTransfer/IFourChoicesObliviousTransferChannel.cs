using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-4 Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo4-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// </summary>
    public interface IFourChoicesObliviousTransferChannel
    {
        Task SendAsync(Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
