using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implements a 1-out-of-4 bit Oblivious Transfer channel on top of any 1-out-of-4 OT channel 
    /// implementation for arbitrary message lengths.
    /// </summary>
    public class FourChoicesBitObliviousTransferChannelAdapter : IFourChoicesBitObliviousTransferChannel
    {
        // todo(lumip): this is almost the same code as in StatelessFourChoicsBitObliviousTransferAdapter.. how to avoid this?
        private IFourChoicesObliviousTransferChannel _generalOt;

        public FourChoicesBitObliviousTransferChannelAdapter(IFourChoicesObliviousTransferChannel generalOt)
        {
            _generalOt = generalOt;
        }

        public IMessageChannel Channel { get { return _generalOt.Channel; } }

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

        public Task<BitArray> ReceiveAsync(QuadrupleIndexArray selectionIndices, int numberOfInvocations)
        {
            return _generalOt.ReceiveAsync(selectionIndices, numberOfInvocations, 1).ContinueWith(
                task => ConvertByteToBitResultMessages(task.Result)
            );
        }

        public Task SendAsync(BitQuadrupleArray options, int numberOfInvocations)
        {
            return _generalOt.SendAsync(ConvertBitToByteOptionMessages(options), numberOfInvocations, 1);
        }
    }
}
