using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-2 bit Correlated Oblivious Transfer channel on top of any 1-out-of-2 C-OT channel 
    /// implementation for arbitrary message lengths.
    /// </summary>
    public class TwoChoicesCorrelatedBitObliviousTransferChannelAdapter : ITwoChoicesCorrelatedBitObliviousTransferChannel
    {

        private ITwoChoicesCorrelatedObliviousTransferChannel _generalOt;

        public TwoChoicesCorrelatedBitObliviousTransferChannelAdapter(ITwoChoicesCorrelatedObliviousTransferChannel generalOt)
        {
            _generalOt = generalOt;
        }

        public IMessageChannel Channel { get { return _generalOt.Channel; } }

        public Task<BitArray> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations)
        {
            return _generalOt.ReceiveAsync(selectionIndices, numberOfInvocations, 1).ContinueWith(
                task => ConvertReceiveOutputByteToBit(task.Result)
            );
        }

        public Task<BitArray> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations)
        {
            return _generalOt.ReceiveAsync(selectionIndices, numberOfInvocations, 1).ContinueWith(
                task => ConvertReceiveOutputByteToBit(task.Result)
            );
        }

        public Task<Pair<Bit>[]> SendAsync(BitArray correlationBits, int numberOfInvocations)
        {
            return _generalOt.SendAsync(ConvertCorrelationBitsToBytes(correlationBits), numberOfInvocations, 1).ContinueWith(
                task => ConvertSendOutputByteToBit(task.Result)
            );
        }

        private byte[][] ConvertCorrelationBitsToBytes(BitArray correlationBits)
        {
            return correlationBits.Select(bit => new[] { (byte)bit }).ToArray();
        }

        private Pair<Bit>[] ConvertSendOutputByteToBit(Pair<byte[]>[] options)
        {
            return options.Select(optionPair => new Pair<Bit>((Bit)optionPair[0][0], (Bit)optionPair[1][0])).ToArray();
        }

        private BitArray ConvertReceiveOutputByteToBit(byte[][] resultMessages)
        {
            return new BitArray(resultMessages.Select(message => (Bit)message[0]).ToArray());
        }
    }
}
