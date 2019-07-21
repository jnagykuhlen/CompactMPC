using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class InsecureObliviousTransfer : StatelessMultiChoicesObliviousTransfer
    {
        protected override Task SendAsyncInternal(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] packedOptions = new byte[numberOfOptions * numberOfInvocations * numberOfMessageBytes];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < numberOfOptions; ++j)
                    Buffer.BlockCopy(options[i][j], 0, packedOptions, (numberOfOptions * i + j) * numberOfMessageBytes, numberOfMessageBytes);
            }

            return channel.WriteMessageAsync(packedOptions);
        }

        protected override async Task<byte[][]> ReceiveAsyncInternal(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
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
    }
}
