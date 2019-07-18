using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class InsecureObliviousTransfer : IStatelessTwoChoicesObliviousTransfer
    {

        public Task SendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: argument sanitization
            byte[] packedOptions = new byte[numberOfOptions * numberOfInvocations * numberOfMessageBytes];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < numberOfOptions; ++j)
                    Buffer.BlockCopy(options[i][j], 0, packedOptions, (numberOfOptions * i + j) * numberOfMessageBytes, numberOfMessageBytes);
            }

            return channel.WriteMessageAsync(packedOptions);
        }

        public async Task<byte[][]> ReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: argument sanitization
            byte[] packedOptions = await channel.ReadMessageAsync();
            if (packedOptions.Length != numberOfOptions * numberOfInvocations * numberOfMessageBytes)
                throw new DesynchronizationException("Received incorrect number of options.");

            byte[][] selectedMessages = new byte[numberOfInvocations][];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                selectedMessages[i] = new byte[numberOfMessageBytes];
                Buffer.BlockCopy(packedOptions, (numberOfOptions * i + selectionIndices[i]) * numberOfMessageBytes, selectedMessages[i], 0, numberOfMessageBytes);
            }

            return selectedMessages;
        }

        public Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return SendAsync(channel, options.Select(pair => pair.ToArray()).ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
