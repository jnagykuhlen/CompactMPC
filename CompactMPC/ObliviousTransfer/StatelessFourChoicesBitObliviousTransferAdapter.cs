using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements stateless 1-out-of-4 bit Oblivious Transfer on top of any 1-out-of-4 OT implementation for
    /// arbitrary message lengths.
    /// </summary>
    public class StatelessFourChoicesBitObliviousTransferChannelAdapter : IStatelessFourChoicesBitObliviousTransfer
    {
        private IStatelessFourChoicesObliviousTransfer _generalOt;

        public StatelessFourChoicesBitObliviousTransferChannelAdapter(IStatelessFourChoicesObliviousTransfer generalOt)
        {
            if (generalOt == null)
                throw new ArgumentNullException(nameof(generalOt));
            _generalOt = generalOt;
        }

        private Quadruple<byte[]>[] ConvertBitToByteOptionMessages(BitQuadrupleArray options)
        {
            Quadruple<byte[]>[] optionMessages = new Quadruple<byte[]>[options.Length];
            for (int i = 0; i < optionMessages.Length; ++i)
            {
                optionMessages[i] = new Quadruple<byte[]>(
                    new[] { (byte)options[i][0] },
                    new[] { (byte)options[i][1] },
                    new[] { (byte)options[i][2] },
                    new[] { (byte)options[i][3] }
                );
            }

            return optionMessages;
        }


        private BitArray ConvertByteToBitResultMessages(byte[][] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Bit)resultMessages[i][0];

            return result;
        }

        public Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations)
        {
            return _generalOt.ReceiveAsync(channel, selectionIndices, numberOfInvocations, 1).ContinueWith(
                task => ConvertByteToBitResultMessages(task.Result)
            );
        }

        public Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations)
        {
            return _generalOt.SendAsync(channel, ConvertBitToByteOptionMessages(options), numberOfInvocations, 1);
        }
    }
}
