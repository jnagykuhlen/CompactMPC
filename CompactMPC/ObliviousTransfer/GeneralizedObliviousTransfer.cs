using System;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public abstract class GeneralizedObliviousTransfer : IObliviousTransfer, IGeneralizedObliviousTransfer
    {
        public Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations)
        {
            return SendAsync(channel, ToOptionMessages(options), numberOfInvocations, 1);
        }

        private Quadruple<byte[]>[] ToOptionMessages(BitQuadrupleArray options)
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

        public Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations)
        {

            return ReceiveAsync(channel, selectionIndices, numberOfInvocations, 1)
                .ContinueWith(task => FromResultMessages(task.Result));
        }

        private BitArray FromResultMessages(byte[][] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Bit)resultMessages[i][0];

            return result;
        }

        public Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.",
                    nameof(options));

            if (options.Flatten().Any(message => message.Length != numberOfMessageBytes))
                throw new ArgumentException("Length of provided options does not match the specified message length.",
                    nameof(options));

            return GeneralizedSendAsync(channel, options, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            return GeneralizedReceiveAsync(channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }

        protected abstract Task GeneralizedSendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract Task<byte[][]> GeneralizedReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
