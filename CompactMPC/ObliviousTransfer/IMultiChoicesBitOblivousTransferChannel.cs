using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-N bit Oblivious Transfer channel implementation.
    /// 
    /// Provides 1ooN-OT for single bits on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// </summary>
    public interface IMultiChoicesBitObliviousTransferChannel
    {
        Task SendAsync(BitArray[][] options, int numberOfOptions, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations);

        Task SendAsync(BitQuadrupleArray options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations);

        /// <summary>
        /// The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.
        /// </summary>
        IMessageChannel Channel { get; }
    }
}
