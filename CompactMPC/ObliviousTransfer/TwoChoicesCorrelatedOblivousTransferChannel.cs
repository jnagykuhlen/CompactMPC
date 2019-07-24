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
    /// Implementation of the 1-out-of-2 Correlated Oblivious Transfer protocol by Asharov et al. based on the OT extension protocol of Ishai et al.
    /// 
    /// Provides 1oo2-C-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// 
    /// The C-OT functionality receives a correlation string ∆ and chooses the sender’s inputs uniformly under the constraint that their XOR equals ∆. [Asharov et al.]
    /// </summary>
    /// <remarks>
    /// References: Yuval Ishai, Joe Kilian, Kobbi Nissim and Erez Petrank: Extending Oblivious Transfers Efficiently. 2003. https://link.springer.com/content/pdf/10.1007/978-3-540-45146-4_9.pdf
    /// and Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public class TwoChoicesCorrelatedExtendedObliviousTransferChannel : TwoChoicesExtendedObliviousTransferChannelBase, ITwoChoicesCorrelatedObliviousTransferChannel
    {
        public TwoChoicesCorrelatedExtendedObliviousTransferChannel(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
            : base(baseOT, securityParameter, cryptoContext) { }

        public async Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (correlationStrings.Length != numberOfInvocations)
                throw new ArgumentException("Amount of provided correlation strings must match the specified number of invocations.", nameof(correlationStrings));

            foreach (byte[] message in correlationStrings)
            {
                if (message.Length != numberOfMessageBytes)
                    throw new ArgumentException("The length of of at least one correlation string does not match the specified message length.", nameof(correlationStrings));
            }

            BitMatrix q = await ReceiveQMatrix(numberOfInvocations);
            Debug.Assert(q.Rows == numberOfInvocations);
            Debug.Assert(q.Cols == SecurityParameter);

            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            byte[][][] maskedOptions = new byte[numberOfInvocations][][];
            //Parallel.For(0, numberOfInvocations, i =>
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                Debug.Assert(correlationStrings[i].Length == numberOfMessageBytes);

                options[i] = new Pair<byte[]>();

                BitArray qRow = q.GetRow((uint)i);

                byte[] query = qRow.ToBytes();
                options[i][0] = RandomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();

                options[i][1] = BitArray.Xor(correlationStrings[i], options[i][0]);

                query = (qRow ^ SenderChoices).ToBytes();

                maskedOptions[i] = new[] { RandomOracle.Mask(options[i][1], query) };
                Debug.Assert(maskedOptions[i][0].Length == numberOfMessageBytes);

            }//);

            await CommunicationTools.WriteOptionsAsync(Channel, maskedOptions, 1, numberOfInvocations, numberOfMessageBytes);

            return options;
        }

        protected override async Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            Debug.Assert(selectionIndices.Length == numberOfInvocations);
            Debug.Assert(tTransposed.Rows == SecurityParameter);
            Debug.Assert(tTransposed.Cols == numberOfInvocations);

            // retrieve the masked options from the sender and unmask the one indicated by the
            //  corresponding selection indices
            byte[][][] maskedOptions = await CommunicationTools.ReadOptionsAsync(Channel, 1, numberOfInvocations, numberOfMessageBytes);
            Debug.Assert(maskedOptions.Length == numberOfInvocations);

            byte[][] results = new byte[numberOfInvocations][];
            //Parallel.For(0, numberOfInvocations, i =>
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                Debug.Assert(maskedOptions[i].Length == 1);
                Debug.Assert(maskedOptions[i][0].Length == numberOfMessageBytes);

                int s = Convert.ToInt32(selectionIndices[i].Value);

                byte[] query = tTransposed.GetColumn((uint)i).ToBytes();

                if (s == 0)
                {
                    results[i] = RandomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
                }
                else
                {
                    results[i] = RandomOracle.Mask(maskedOptions[i][0], query);
                }
            }//);
            return results;
        }
    }
}
