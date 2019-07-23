using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-N OT channel provider that deals out channels for a stateless OT implementation.
    /// </summary>
    public class StatelessMultiChoicesObliviousTransferProvider : IObliviousTransferProvider<IMultiChoicesObliviousTransferChannel>
    {
        // todo(lumip): this is exactly the same code as for 1oo2, except for the method signatures below..
        //  how to avoid this duplication??
        private IStatelessMultiChoicesObliviousTransfer _statelessOT;

        public StatelessMultiChoicesObliviousTransferProvider(IStatelessMultiChoicesObliviousTransfer statelessOT)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;
        }

        public IMultiChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new StatelessMultiChoicesObliviousTransferChannel(_statelessOT, channel);
        }
    }
}
