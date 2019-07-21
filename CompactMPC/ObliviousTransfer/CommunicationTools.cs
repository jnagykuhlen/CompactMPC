using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A collection of communication helper functions for Extended Oblivious Transfer implementations.
    /// 
    /// See <see cref="TwoChoicesExtendedObliviousTransferChannel"/>, <see cref="TwoChoicesCorrelatedExtendedObliviousTransferChannel"/>, <see cref="TwoChoicesRandomExtendedObliviousTransferChannel"/>
    /// </summary>
    internal class CommunicationTools
    {
        public static Task WriteOptionsAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] messageBuffer = new byte[numberOfOptions * numberOfMessageBytes * numberOfInvocations];
            Parallel.For(0, numberOfInvocations, i =>
            {
                for (int j = 0; j < numberOfOptions; ++j)
                {
                    Buffer.BlockCopy(
                        options[i][j],
                        0,
                        messageBuffer,
                        (numberOfOptions * numberOfMessageBytes) * i + j * numberOfMessageBytes,
                        numberOfMessageBytes
                    );
                }
            });

            return channel.WriteMessageAsync(messageBuffer);
        }

        public static async Task<byte[][][]> ReadOptionsAsync(IMessageChannel channel, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] messageBuffer = await channel.ReadMessageAsync();
            byte[][][] result = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                result[i] = new byte[numberOfOptions][];
                for (int j = 0; j < numberOfOptions; ++j)
                {
                    result[i][j] = new byte[numberOfMessageBytes];
                    Buffer.BlockCopy(
                        messageBuffer,
                        (numberOfOptions * numberOfMessageBytes) * i + j * numberOfMessageBytes,
                        result[i][j],
                        0,
                        numberOfMessageBytes
                    );
                }
            });
            return result;
        }
    }
}
