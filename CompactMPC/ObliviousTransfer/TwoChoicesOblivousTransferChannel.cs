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
    public class TwoChoicesExtendedObliviousTransferChannel : TwoChoicesExtendedObliviousTransferChannelBase, ITwoChoicesObliviousTransferChannel
    {
        public TwoChoicesExtendedObliviousTransferChannel(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
            : base(baseOT, securityParameter, cryptoContext) { }

        protected override async Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // retrieve the masked options from the sender and unmask the one indicated by the
            //  corresponding selection indices
            byte[][][] maskedOptions = await CommunicationTools.ReadOptionsAsync(Channel, 2, numberOfInvocations, numberOfMessageBytes);

            byte[][] results = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _receiverState.InvocationCounter + (uint)i;
                int s = Convert.ToInt32(selectionIndices[i].Value);

                byte[] query = BufferBuilder.From(tTransposed.GetColumn((uint)i).ToBytes()).With((int)invocationIndex).With(s).Create();
                results[i] = _randomOracle.Mask(maskedOptions[i][s], query);
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

            byte[][][] maskedOptions = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _senderState.InvocationCounter + (uint)i;
                maskedOptions[i] = new byte[2][];
                BitArray qRow = q.GetRow((uint)i);
                for (int j = 0; j < 2; ++j)
                {
                    if (j == 1)
                        qRow.Xor(_senderState.RandomChoices);
                    byte[] query = BufferBuilder.From(qRow.ToBytes()).With((int)invocationIndex).With(j).Create();
                    maskedOptions[i][j] = _randomOracle.Mask(options[i][j], query);
                }
            });

            _senderState.InvocationCounter += (uint)numberOfInvocations;
            await CommunicationTools.WriteOptionsAsync(Channel, maskedOptions, 2, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
