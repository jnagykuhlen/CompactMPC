using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-2 bit Random Oblivious Transfer channel on top of any 1-out-of-2 R-OT channel 
    /// implementation for arbitrary message lengths.
    /// </summary>
    public class TwoChoicesRandomBitObliviousTransferChannelAdapter : ITwoChoicesRandomBitObliviousTransferChannel
    {

        private ITwoChoicesRandomObliviousTransferChannel _generalOt;

        public TwoChoicesRandomBitObliviousTransferChannelAdapter(ITwoChoicesRandomObliviousTransferChannel generalOt)
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

        public Task<Pair<Bit>[]> SendAsync(int numberOfInvocations)
        {
            return _generalOt.SendAsync(numberOfInvocations, 1).ContinueWith(
                task => ConvertSendOutputByteToBit(task.Result)
            );
        }

        private Pair<Bit>[] ConvertSendOutputByteToBit(Pair<byte[]>[] options)
        {
            Pair<Bit>[] outputs = new Pair<Bit>[options.Length];
            for (int i = 0; i < outputs.Length; ++i)
            {
                outputs[i] = new Pair<Bit>(
                    (Bit)options[i][0][0],
                    (Bit)options[i][1][0]
                );
            }

            return outputs;
        }

        private BitArray ConvertReceiveOutputByteToBit(byte[][] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Bit)resultMessages[i][0];

            return result;
        }
    }
}
