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
            Debug.Assert(q.Rows == numberOfInvocations);
            Debug.Assert(q.Cols == SecurityParameter);

            Pair<byte[]>[] options = new Pair<byte[]>[numberOfInvocations];
            //Parallel.For(0, numberOfInvocations, i =>
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                options[i] = new Pair<byte[]>();
                BitArray qRow = q.GetRow((uint)i);
                for (int j = 0; j < 2; ++j)
                {
                    if (j == 1)
                        qRow.Xor(SenderChoices);
                    byte[] query = qRow.ToBytes();
                    options[i][j] = RandomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
                }
            }//);

            return options;
        }

        protected override Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes)
        {
            Debug.Assert(selectionIndices.Length == numberOfInvocations);
            Debug.Assert(tTransposed.Rows == SecurityParameter);
            Debug.Assert(tTransposed.Cols == numberOfInvocations);
            
            byte[][] results = new byte[numberOfInvocations][];
            //Parallel.For(0, numberOfInvocations, i =>
            for (int i = 0; i < numberOfInvocations; ++i)
            {
                int s = Convert.ToInt32(selectionIndices[i].Value);

                byte[] query = tTransposed.GetColumn((uint)i).ToBytes();
                results[i] = RandomOracle.Invoke(query).Take(numberOfMessageBytes).ToArray();
            }//);

            return Task.FromResult(results);
        }
    }
}
