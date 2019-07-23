using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-4 bit Oblivious Transfer channel provider using any 1-out-of-4 OT channel 
    /// implementation for arbitrary message lengths and wrapping the channels it returns into
    /// <see cref="FourChoicesBitObliviousTransferChannelAdapter"/>.
    /// </summary>
    public class FourChoicesBitObliviousTransferProviderAdapter : IFourChoicesBitObliviousTransferProvider
    {
        private IFourChoicesObliviousTransferProvider _otProvider;

        public FourChoicesBitObliviousTransferProviderAdapter(IFourChoicesObliviousTransferProvider otProvider)
        {
            _otProvider = otProvider;
        }

        public IFourChoicesBitObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new FourChoicesBitObliviousTransferChannelAdapter(_otProvider.CreateChannel(channel));
        }
    }
}
