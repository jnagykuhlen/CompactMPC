using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A stateless 1-out-of-2 Oblivious Transfer implementation.
    /// 
    /// Stateless here means that the OT implementation does not maintain state for each channel (i.e., pair of communicating parties)
    /// in-between invocations.
    /// </summary>
    public interface IStatelessTwoChoicesObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
