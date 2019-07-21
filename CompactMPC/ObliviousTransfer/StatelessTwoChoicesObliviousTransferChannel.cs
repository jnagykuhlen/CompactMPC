using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A wrapper turning a stateless 1-out-of-2 OT implementation into an OT channel.
    /// </summary>
    public class StatelessTwoChoicesObliviousObliviousTransferChannel : ITwoChoicesObliviousTransferChannel
    {
        // todo(lumip): this is exactly the same code as for 1oo4, 1ooN, except for the method signatures below..
        //  how to avoid this duplication??

        private IStatelessTwoChoicesObliviousTransfer _statelessOT;
        public IMessageChannel Channel { get; private set; }

        public StatelessTwoChoicesObliviousObliviousTransferChannel(IStatelessTwoChoicesObliviousTransfer statelessOT, IMessageChannel channel)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            Channel = channel;
        }

        public Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.ReceiveAsync(Channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.ReceiveAsync(Channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.SendAsync(Channel, options, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
