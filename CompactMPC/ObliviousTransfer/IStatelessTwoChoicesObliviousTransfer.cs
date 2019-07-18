using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A stateless 1-out-of-2 Oblivious Transfer implementation.
    /// 
    /// Stateless here means that the OT implementation does not maintain state for each channel (i.e., pair of communicating parties)
    /// in-between invocations.
    /// </summary>
    public interface IStatelessTwoChoicesObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }

    /// <summary>
    /// A wrapper turning a stateless 1-out-of-2 OT implementation into an OT channel.
    /// </summary>
    public class StatelessTwoChoiceObliviousObliviousTransferChannelAdapter : ITwoChoicesObliviousTransferChannel
    {
        private IStatelessTwoChoicesObliviousTransfer _statelessOT;
        public IMessageChannel Channel { get; private set; }

        public StatelessTwoChoiceObliviousObliviousTransferChannelAdapter(IStatelessTwoChoicesObliviousTransfer statelessOT, IMessageChannel channel)
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

    /// <summary>
    /// A 1-out-of-2 OT channel provider that deals out channels for a stateless OT implementation.
    /// </summary>
    public class StatelessTwoChoicesObliviousTransferChannelProvider : ITwoChoicesObliviousTransferProvider
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
            return new StatelessTwoChoiceObliviousObliviousTransferChannelAdapter(_statelessOT, channel);
        }
    }
}
