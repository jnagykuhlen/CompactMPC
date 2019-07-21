using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-N Oblivious Transfer channel implementation.
    /// 
    /// Provides 1ooN-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// </summary>
    public interface IMultiChoicesObliviousTransferChannel
    {
        Task SendAsync(byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
    }
}
