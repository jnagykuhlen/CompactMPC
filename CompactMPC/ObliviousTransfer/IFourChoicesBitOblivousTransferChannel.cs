using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-4 bit Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo4-OT for single bits on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// </summary>
    public interface IFourChoicesBitObliviousTransferChannel
    {
        Task SendAsync(BitQuadrupleArray options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations);

        /// <summary>
        /// The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.
        /// </summary>
        IMessageChannel Channel { get; }
    }
}
