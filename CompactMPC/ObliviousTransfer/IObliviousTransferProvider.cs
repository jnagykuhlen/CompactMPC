using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Provides stateful Oblivious Transfer channel instances of the specific type indicated by the template argument
    /// for pairs of parties identified by network channels.
    /// </summary>
    public interface IObliviousTransferProvider<ObliviousTransferChannelT>
    {
        // todo(lumip: somehow constraint the template type so only OT channels can be used? but on what common characteristic?

        /// <summary>
        /// Creates a stateful Oblivious Transfer channel instance for the given network channel.
        /// </summary>
        /// <param name="channel">The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.</param>
        /// <returns>The stateful Random Oblivious Transfer channel.</returns>
        ObliviousTransferChannelT CreateChannel(IMessageChannel channel);
    }
}
