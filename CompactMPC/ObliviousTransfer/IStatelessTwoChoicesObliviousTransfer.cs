using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IStatelessTwoChoicesObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }

    public class StatelessTwoChoiceObliviousObliviousTransferChannelDecorator : ITwoChoicesObliviousTransferChannel
    {
        private IStatelessTwoChoicesObliviousTransfer _statelessOT;
        public IMessageChannel Channel { get; private set; }

        public StatelessTwoChoiceObliviousObliviousTransferChannelDecorator(IStatelessTwoChoicesObliviousTransfer statelessOT, IMessageChannel channel)
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

    public class StatelessTwoChoicesObliviousTransferChannelProvider : ITwoChoicesObliviousTransfer
    {
        private IStatelessTwoChoicesObliviousTransfer _statelessOT;

        public StatelessTwoChoicesObliviousTransferChannelProvider(IStatelessTwoChoicesObliviousTransfer statelessOT)
        {
            if (statelessOT == null)
                throw new ArgumentNullException(nameof(statelessOT));
            _statelessOT = statelessOT;
        }

        public ITwoChoicesObliviousTransferChannel CreateChannel(IMessageChannel channel)
        {
            return new StatelessTwoChoiceObliviousObliviousTransferChannelDecorator(_statelessOT, channel);
        }
    }
}
