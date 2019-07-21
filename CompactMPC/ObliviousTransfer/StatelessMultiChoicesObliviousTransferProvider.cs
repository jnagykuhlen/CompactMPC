using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-N OT channel provider that deals out channels for a stateless OT implementation.
    /// </summary>
    public class StatelessMultiChoicesObliviousTransferProvider : IMultiChoicesObliviousTransferProvider
    {
        // todo(lumip): this is exactly the same code as for 1oo4, 1ooN, except for the method signatures below..
        //  how to avoid this duplication??
        private IStatelessMultiChoicesObliviousTransfer _statelessOT;

        public StatelessMultiChoicesObliviousTransferProvider(IStatelessMultiChoicesObliviousTransfer statelessOT)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;
        }

        IMultiChoicesObliviousTransferChannel IMultiChoicesObliviousTransferProvider.CreateChannel(IMessageChannel channel)
        {
            return new StatelessMultiChoicesObliviousObliviousTransferChannel(_statelessOT, channel);
        }
    }
}
