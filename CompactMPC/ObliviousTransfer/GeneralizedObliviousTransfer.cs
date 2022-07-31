using System;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public abstract class GeneralizedObliviousTransfer : IObliviousTransfer, IGeneralizedObliviousTransfer
    {
        public Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations)
        {
            return SendAsync(channel, ToOptionMessages(options), numberOfInvocations, 1);
        }

        private static Quadruple<Message>[] ToOptionMessages(BitQuadrupleArray options)
        {
            Quadruple<Message>[] optionMessages = new Quadruple<Message>[options.Length];
            for (int i = 0; i < optionMessages.Length; ++i)
            {
                optionMessages[i] = new Quadruple<Message>(
                    ToOptionMessage(options[i][0]),
                    ToOptionMessage(options[i][1]),
                    ToOptionMessage(options[i][2]),
                    ToOptionMessage(options[i][3])
                );
            }

            return optionMessages;
        }

        private static Message ToOptionMessage(Bit bit)
        {
            return new Message(new[] { (byte)bit });
        }

        public Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations)
        {
            return ReceiveAsync(channel, selectionIndices, numberOfInvocations, 1)
                .ContinueWith(task => FromResultMessages(task.Result));
        }

        private static BitArray FromResultMessages(Message[] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = FromResultMessage(resultMessages[i]);

            return result;
        }

        private static Bit FromResultMessage(Message message)
        {
            return (Bit)message.ToBuffer()[0];
        }

        public Task SendAsync(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.",
                    nameof(options));

            if (options.Flatten().Any(message => message.Length != numberOfMessageBytes))
                throw new ArgumentException("Length of provided options does not match the specified message length.",
                    nameof(options));

            return GeneralizedSendAsync(channel, options, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<Message[]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices,
            int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException(
                    "Provided selection indices must match the specified number of invocations.",
                    nameof(selectionIndices));

            return GeneralizedReceiveAsync(channel, selectionIndices, numberOfInvocations, numberOfMessageBytes);
        }

        protected abstract Task GeneralizedSendAsync(IMessageChannel channel, Quadruple<Message>[] options,
            int numberOfInvocations, int numberOfMessageBytes);

        protected abstract Task<Message[]> GeneralizedReceiveAsync(IMessageChannel channel,
            QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
