using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Insecure (non-oblivious) implementation of the oblivious transfer interface.
    /// 
    /// Caution! This class is intended for testing and debugging purposes and does not provide any security.
    /// Hence, it should not be used with any sensitive or in production deployments. All options are SENT IN THE PLAIN.
    /// </summary>
    public class InsecureObliviousTransfer : GeneralizedObliviousTransfer
    {
        protected override Task GeneralizedSendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] packedOptions = new byte[4 * numberOfInvocations * numberOfMessageBytes];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < 4; ++j)
                    Buffer.BlockCopy(options[i][j], 0, packedOptions, (4 * i + j) * numberOfMessageBytes, numberOfMessageBytes);
            }

            return channel.WriteMessageAsync(packedOptions);
        }

        protected override async Task<byte[][]> GeneralizedReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] packedOptions = await channel.ReadMessageAsync();
            if (packedOptions.Length != 4 * numberOfInvocations * numberOfMessageBytes)
                throw new DesynchronizationException("Received incorrect number of options.");

            byte[][] selectedMessages = new byte[numberOfInvocations][];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                selectedMessages[i] = new byte[numberOfMessageBytes];
                Buffer.BlockCopy(packedOptions, (4 * i + selectionIndices[i]) * numberOfMessageBytes, selectedMessages[i], 0, numberOfMessageBytes);
            }

            return selectedMessages;
        }
    }
}
