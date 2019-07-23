using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using CompactMPC.Networking;
using CompactMPC.Buffers;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Implementation of the OT extension protocol that enables extending a small number of expensive oblivious transfer
    /// invocation (so called "base OTs") into a larger number of usable oblivious transfer invocations using only cheap
    /// symmetric cryptography via a random oracle instantiation.
    /// </summary>
    /// <remarks>
    /// References: Yuval Ishai, Joe Kilian, Kobbi Nissim and Erez Petrank: Extending Oblivious Transfers Efficiently. 2003. https://link.springer.com/content/pdf/10.1007/978-3-540-45146-4_9.pdf
    /// and Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public class TwoChoicesExtendedObliviousTransferChannel : TwoChoicesExtendedObliviousTransferChannelBase, ITwoChoicesObliviousTransferChannel
    {
        public TwoChoicesExtendedObliviousTransferChannel(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
            : base(baseOT, securityParameter, cryptoContext) { }

        protected override async Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            Debug.Assert(selectionIndices.Length == numberOfInvocations);
            Debug.Assert(tTransposed.Rows == SecurityParameter);
            Debug.Assert(tTransposed.Cols == numberOfInvocations);
            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // retrieve the masked options from the sender and unmask the one indicated by the
            //  corresponding selection indices
            byte[][][] maskedOptions = await CommunicationTools.ReadOptionsAsync(Channel, 2, numberOfInvocations, numberOfMessageBytes);
            Debug.Assert(maskedOptions.Length == numberOfInvocations);

            byte[][] results = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                int s = Convert.ToInt32(selectionIndices[i].Value);

                Debug.Assert(maskedOptions[i][s].Length == numberOfMessageBytes);
                byte[] query = tTransposed.GetColumn((uint)i).ToBytes();
                results[i] = RandomOracle.Mask(maskedOptions[i][s], query);
            });
            return results;
        }

        public async Task SendAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Amount of provided option pairs must match the specified number of invocations.", nameof(options));

            for (int j = 0; j < options.Length; ++j)
            {
                foreach (byte[] message in options[j])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("The length of of at least one option does not match the specified message length.", nameof(options));
                }
            }

            BitMatrix q = await ReceiveQMatrix(numberOfInvocations);
            Debug.Assert(q.Rows == numberOfInvocations);
            Debug.Assert(q.Cols == SecurityParameter);

            byte[][][] maskedOptions = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                maskedOptions[i] = new byte[2][];
                BitArray qRow = q.GetRow((uint)i);
                for (int j = 0; j < 2; ++j)
                {
                    Debug.Assert(options[i][j].Length == numberOfMessageBytes);
                    if (j == 1)
                        qRow.Xor(SenderChoices);
                    byte[] query = qRow.ToBytes();
                    maskedOptions[i][j] = RandomOracle.Mask(options[i][j], query);
                }
            });

            await CommunicationTools.WriteOptionsAsync(Channel, maskedOptions, 2, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
