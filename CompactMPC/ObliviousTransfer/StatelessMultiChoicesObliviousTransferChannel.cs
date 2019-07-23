using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A wrapper turning a stateless 1-out-of-N OT implementation into an OT channel.
    /// </summary>
    public class StatelessMultiChoicesObliviousTransferChannel : IMultiChoicesObliviousTransferChannel
    {
        // todo(lumip): this is exactly the same code as for 1oo2, except for the method signatures below..
        //  how to avoid this duplication??

        private IStatelessMultiChoicesObliviousTransfer _statelessOT;
        public IMessageChannel Channel { get; private set; }

        public StatelessMultiChoicesObliviousTransferChannel(IStatelessMultiChoicesObliviousTransfer statelessOT, IMessageChannel channel)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;

            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            Channel = channel;
        }

        public Task SendAsync(byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.SendAsync(Channel, options, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.ReceiveAsync(Channel, selectionIndices, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.SendAsync(Channel, options, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return _statelessOT.ReceiveAsync(Channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
