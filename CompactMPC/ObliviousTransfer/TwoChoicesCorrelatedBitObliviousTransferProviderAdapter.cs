using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-2 bit Correlated Oblivious Transfer channel provider using any 1-out-of-2 C-OT provider
    /// implementation for arbitrary message lengths and wrapping the channels it returns into
    /// <see cref="TwoChoicesCorrelatedBitObliviousTransferChannelAdapter"/>.
    /// </summary>
    public class TwoChoicesCorrelatedBitObliviousTransferProviderAdapter : IObliviousTransferProvider<ITwoChoicesCorrelatedBitObliviousTransferChannel>
    {
        private IObliviousTransferProvider<ITwoChoicesCorrelatedObliviousTransferChannel> _otProvider;

        public TwoChoicesCorrelatedBitObliviousTransferProviderAdapter(IObliviousTransferProvider<ITwoChoicesCorrelatedObliviousTransferChannel> otProvider)
        {
            _otProvider = otProvider;
        }

        public ITwoChoicesCorrelatedBitObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new TwoChoicesCorrelatedBitObliviousTransferChannelAdapter(_otProvider.CreateChannel(channel));
        }
    }
}
