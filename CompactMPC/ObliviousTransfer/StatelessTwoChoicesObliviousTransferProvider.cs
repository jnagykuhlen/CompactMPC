using System;
using System.Collections.Generic;
using System.Text;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-2 OT channel provider that deals out channels for a stateless OT implementation.
    /// </summary>
    public class StatelessTwoChoicesObliviousTransferProvider : ITwoChoicesObliviousTransferProvider
    {
        // todo(lumip): this is exactly the same code as for 1oo4, 1ooN, except for the method signatures below..
        //  how to avoid this duplication??

        private IStatelessTwoChoicesObliviousTransfer _statelessOT;

        public StatelessTwoChoicesObliviousTransferProvider(IStatelessTwoChoicesObliviousTransfer statelessOT)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;
        }

        public ITwoChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new StatelessTwoChoicesObliviousTransferChannel(_statelessOT, channel);
        }
    }
}
