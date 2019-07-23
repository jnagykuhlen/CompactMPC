using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Provides 1-out-of-4 stateful bit Oblivious Transfer channel instances for pairs of parties identified by network channels.
    /// </summary>
    public interface IFourChoicesBitObliviousTransferProvider
    {
        /// <summary>
        /// Creates a 1-out-of-4 stateful bit Oblivious Transfer channel instance for the given network channel.
        /// </summary>
        /// <param name="channel">The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.</param>
        /// <returns>The 1-out-of-4 stateful Oblivious Transfer channel.</returns>
        IFourChoicesBitObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }
}
