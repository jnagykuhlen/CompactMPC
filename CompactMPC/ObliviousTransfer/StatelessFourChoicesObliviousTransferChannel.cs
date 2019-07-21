using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A wrapper turning a stateless 1-out-of-4 OT implementation into an OT channel.
    /// </summary>
    public class StatelessFourChoicesObliviousObliviousTransferChannel : IFourChoicesObliviousTransferChannel
    {
        // todo(lumip): this is exactly the same code as for 1oo2, 1ooN, except for the method signatures below..
        //  how to avoid this duplication??

        private IStatelessFourChoicesObliviousTransfer _statelessOT;
        public IMessageChannel Channel { get; private set; }

        public StatelessFourChoicesObliviousObliviousTransferChannel(IStatelessFourChoicesObliviousTransfer statelessOT, IMessageChannel channel)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            Channel = channel;
        }

        public Task<byte[][]> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.ReceiveAsync(Channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.SendAsync(Channel, options, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
