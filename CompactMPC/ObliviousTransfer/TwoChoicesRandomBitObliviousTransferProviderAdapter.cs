using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-2 bit Random Oblivious Transfer channel provider using any 1-out-of-2 R-OT provider
    /// implementation for arbitrary message lengths and wrapping the channels it returns into
    /// <see cref="TwoChoicesRandomBitObliviousTransferChannelAdapter"/>.
    /// </summary>
    public class TwoChoicesRandomBitObliviousTransferProviderAdapter : IObliviousTransferProvider<ITwoChoicesRandomBitObliviousTransferChannel>
    {
        private IObliviousTransferProvider<ITwoChoicesRandomObliviousTransferChannel> _otProvider;

        public TwoChoicesRandomBitObliviousTransferProviderAdapter(IObliviousTransferProvider<ITwoChoicesRandomObliviousTransferChannel> otProvider)
        {
            _otProvider = otProvider;
        }

        public ITwoChoicesRandomBitObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new TwoChoicesRandomBitObliviousTransferChannelAdapter(_otProvider.CreateChannel(channel));
        }
    }
}
