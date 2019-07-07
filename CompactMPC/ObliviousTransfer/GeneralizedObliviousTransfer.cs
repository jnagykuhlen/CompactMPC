using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return SendAsync(channel, options.Select(quad => quad.ToArray()).ToArray(), 4, numberOfInvocations, numberOfMessageBytes);
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
            return ReceiveAsync(channel, selectionIndices, numberOfInvocations, 1).ContinueWith(task => FromResultMessages(task.Result));
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToArray(), 4, numberOfInvocations, numberOfMessageBytes);
        }

        private BitArray FromResultMessages(byte[][] resultMessages)
        {
            BitArray result = new BitArray(resultMessages.Length);
            for (int i = 0; i < result.Length; ++i)
                result[i] = (Bit)resultMessages[i][0];

            return result;
        }
        
        public Task SendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

            for (int j = 0; j < options.Length; ++j)
            {
                if (options[j].Length != numberOfOptions)
                    throw new ArgumentException("Each per-invocation options tuple must provide the specified number of options.", nameof(options));

                foreach (byte[] message in options[j])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("Length of provided options does not match the specified message length.", nameof(options));
                }
            }

            return GeneralizedSendAsync(channel, options, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            for (int i = 0; i < selectionIndices.Length; ++i)
            {
                if (selectionIndices[i] >= numberOfOptions)
                    throw new ArgumentException("Provided selection indices must not exceed the number of available options.", nameof(numberOfOptions));
            }

            return GeneralizedReceiveAsync(channel, selectionIndices, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        protected abstract Task GeneralizedSendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract Task<byte[][]> GeneralizedReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);

        public async Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            await SendAsync(channel, options.Select(pair => pair.ToArray()).ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }

        public async Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return await ReceiveAsync(channel, selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
        }

        public async Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return await ReceiveAsync(channel, selectionIndices.ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }

    }
}
