using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public class UnsafeObliviousTransfer : GeneralizedObliviousTransfer
    {
        public override Task SendAsync(Stream stream, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));

            for (int i = 0; i < options.Length; ++i)
            {
                foreach (byte[] message in options[i])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("Length of provided options does not match the specified message length.", nameof(options));
                }
            }

            byte[] packedOptions = new byte[4 * numberOfInvocations * numberOfMessageBytes];
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                for (int j = 0; j < 4; ++j)
                    Buffer.BlockCopy(options[i][j], 0, packedOptions, (4 * i + j) * numberOfMessageBytes, numberOfMessageBytes);
            }

            return stream.WriteAsync(packedOptions);
        }

        public override async Task<byte[][]> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            for (int i = 0; i < selectionIndices.Length; ++i)
            {
                if (selectionIndices[i] < 0 || selectionIndices[i] >= 4)
                    throw new ArgumentOutOfRangeException(nameof(selectionIndices), "Invalid selection index for 1-out-of-4 oblivious transfer.");
            }

            byte[] packedOptions = await stream.ReadAsync(4 * numberOfInvocations * numberOfMessageBytes);

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
