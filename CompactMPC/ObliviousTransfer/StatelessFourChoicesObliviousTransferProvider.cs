using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-4 OT channel provider that deals out channels for a stateless OT implementation.
    /// </summary>
    public class StatelessFourChoicesObliviousTransferProvider : IFourChoicesObliviousTransferProvider
    {
        // todo(lumip): this is exactly the same code as for 1oo2, 1ooN, except for the method signatures below..
        //  how to avoid this duplication??

        private IStatelessFourChoicesObliviousTransfer _statelessOT;

        public StatelessFourChoicesObliviousTransferProvider(IStatelessFourChoicesObliviousTransfer statelessOT)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;
        }

        public IFourChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new StatelessFourChoicesObliviousObliviousTransferChannel(_statelessOT, channel);
        }
    }
}
