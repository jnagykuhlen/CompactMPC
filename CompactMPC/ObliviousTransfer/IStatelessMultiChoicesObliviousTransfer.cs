using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A stateless 1-out-of-N Oblivious Transfer implementation.
    /// 
    /// Stateless here means that the OT implementation does not maintain state for each channel (i.e., pair of communicating parties)
    /// in-between invocations.
    /// </summary>
    public interface IStatelessMultiChoicesObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
    }
}
