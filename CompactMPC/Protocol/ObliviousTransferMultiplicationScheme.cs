using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol
{
    public class ObliviousTransferMultiplicationScheme : IPairwiseMultiplicationScheme
    {
        private IObliviousTransfer _obliviousTransfer;
        private RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferMultiplicationScheme(IObliviousTransfer obliviousTransfer, CryptoContext cryptoContext)
        {
            _obliviousTransfer = obliviousTransfer;
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        public async Task<BitArray> ComputeMultiplicationSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // Given local shares x1, y1
            // compute share of x1 * y1 + x1 * y2 + x2 * y1 + x2 * y2
            BitArray partialMultiplicationShares = await ComputePartialMultiplicationSharesAsync(session, leftShares, rightShares, numberOfInvocations);
            BitArray localMultiplicationShares = leftShares & rightShares;

            return partialMultiplicationShares ^ localMultiplicationShares;
        }

        private Task<BitArray> ComputePartialMultiplicationSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // Given local shares x1, y1
            // compute share of x1 * y2 + x2 * y1
            if (session.RemoteParty.Id < session.LocalParty.Id)
            {
                return ComputePartialSenderSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations);
            }
            else
            {
                return ComputePartialReceiverSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations);
            }
        }

        private async Task<BitArray> ComputePartialSenderSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            BitArray randomShares = _randomNumberGenerator.GetBits(numberOfInvocations);
            BitQuadrupleArray options = new BitQuadrupleArray(numberOfInvocations);

            for (int i = 0; i < numberOfInvocations; ++i)
            {
                options[i] = new BitQuadruple(
                    randomShares[i],                                     // 00
                    randomShares[i] ^ leftShares[i],                     // 01
                    randomShares[i] ^ rightShares[i],                    // 10
                    randomShares[i] ^ leftShares[i] ^ rightShares[i]     // 11
                );
            }

            await _obliviousTransfer.SendAsync(channel, options, numberOfInvocations);
            return randomShares;
        }

        private Task<BitArray> ComputePartialReceiverSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            QuadrupleIndexArray selectionIndices = new QuadrupleIndexArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
                selectionIndices[i] = 2 * (byte)leftShares[i] + (byte)rightShares[i];

            return _obliviousTransfer.ReceiveAsync(channel, selectionIndices, numberOfInvocations);
        }
    }
}
