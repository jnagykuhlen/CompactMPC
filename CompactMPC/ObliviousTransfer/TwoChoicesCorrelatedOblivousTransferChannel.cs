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

            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            byte[][][] maskedOptions = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                Debug.Assert(correlationStrings[i].Length == numberOfMessageBytes);

                uint invocationIndex = _senderState.InvocationCounter + (uint)i;
                options[i] = new Pair<byte[]>();

                BitArray qRow = q.GetRow((uint)i);

                byte[] query = qRow.ToBytes(); // todo: should add invocation index?
                options[i][0] = _randomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();

                options[i][1] = BitArray.Xor(correlationStrings[i], options[i][0]);

                query = (qRow ^ _senderState.RandomChoices).ToBytes();

                maskedOptions[i] = new[] { _randomOracle.Mask(options[i][1], query) };
                Debug.Assert(maskedOptions[i][0].Length == numberOfMessageBytes);

            });

            _senderState.InvocationCounter += (uint)numberOfInvocations;
            await CommunicationTools.WriteOptionsAsync(Channel, maskedOptions, 1, numberOfInvocations, numberOfMessageBytes);

            return options;
        }

        protected override async Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            // retrieve the masked options from the sender and unmask the one indicated by the
            //  corresponding selection indices
            byte[][][] maskedOptions = await CommunicationTools.ReadOptionsAsync(Channel, 1, numberOfInvocations, numberOfMessageBytes);
            Debug.Assert(maskedOptions.Length == numberOfInvocations);

            byte[][] results = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                Debug.Assert(maskedOptions[i].Length == 1);
                Debug.Assert(maskedOptions[i][0].Length == numberOfMessageBytes);

                uint invocationIndex = _receiverState.InvocationCounter + (uint)i;
                int s = Convert.ToInt32(selectionIndices[i].Value);

                Debug.Assert(s >= 0 && s <= 1);

                byte[] query = tTransposed.GetColumn((uint)i).ToBytes();

                if (s == 0)
                {
                    results[i] = _randomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
                }
                else
                {
                    results[i] = _randomOracle.Mask(maskedOptions[i][0], query);
                }
            });
            return results;
        }
    }
}
