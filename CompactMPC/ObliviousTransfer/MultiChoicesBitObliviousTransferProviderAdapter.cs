using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-N bit Oblivious Transfer channel provider using any 1-out-of-N OT channel 
    /// implementation for arbitrary message lengths and wrapping the channels it returns into
    /// <see cref="MultiChoicesBitObliviousTransferChannelAdapter"/>.
    /// </summary>
    public class MultiChoicesBitObliviousTransferProviderAdapter : IMultiChoicesBitObliviousTransferProvider
    {
        private IMultiChoicesObliviousTransferProvider _otProvider;

        public MultiChoicesBitObliviousTransferProviderAdapter(IMultiChoicesObliviousTransferProvider otProvider)
        {
            _otProvider = otProvider;
        }

        public IMultiChoicesBitObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new MultiChoicesBitObliviousTransferChannelAdapter(_otProvider.CreateChannel(channel));
        }
    }
}
