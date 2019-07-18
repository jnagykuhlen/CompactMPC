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
    public class TwoChoicesRandomExtendedObliviousTransferChannel : TwoChoicesExtendedObliviousTransferChannelBase, ITwoChoicesRandomObliviousTransferChannel
    {
        public TwoChoicesRandomExtendedObliviousTransferChannel(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
            : base(baseOT, securityParameter, cryptoContext) { }

        public async Task<Pair<byte[]>[]> SendAsync(int numberOfInvocations, int numberOfMessageBytes)
        {
            BitMatrix q = await ReceiveQMatrix(numberOfInvocations);

            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _senderState.InvocationCounter + (uint)i;
                options[i] = new Pair<byte[]>();
                BitArray qRow = q.GetRow((uint)i);
                for (int j = 0; j < 2; ++j)
                {
                    if (j == 1)
                        qRow.Xor(_senderState.RandomChoices);
                    byte[] query = BufferBuilder.From(qRow.ToBytes()).With((int)invocationIndex).With(j).Create();
                    options[i][j] = _randomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
                }
            });
            _senderState.InvocationCounter += (uint)numberOfInvocations;

            return options;
        }

        protected override async Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[][] results = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _receiverState.InvocationCounter + (uint)i;
                int s = Convert.ToInt32(selectionIndices[i].Value);

                byte[] query = BufferBuilder.From(tTransposed.GetColumn((uint)i).ToBytes()).With((int)invocationIndex).With(s).Create();
                results[i] = _randomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
            });

            return results;
        }
    }
}
