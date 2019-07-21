using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Provides 1-out-of-2 stateful Random Oblivious Transfer channel instances for pairs of parties identified by network channels.
    /// </summary>
    public interface ITwoChoicesRandomObliviousTransferProvider
    {
        /// <summary>
        /// Creates an 1-out-of-2 stateful Random Oblivious Transfer channel instance for the given network channel.
        /// </summary>
        /// <param name="channel">The network channel the R-OT operates on, uniquely identifying the pair of parties involved in the R-OT.</param>
        /// <returns>The 1-out-of-2 stateful Random Oblivious Transfer channel.</returns>
        ITwoChoicesRandomObliviousTransferChannel CreateChannel(IMessageChannel channel);
    }
}
