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

        public Task<BitArray> ComputeMultiplicationSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            if (session.RemoteParty.Id < session.LocalParty.Id)
            {
                return ComputeSenderSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations);
            }
            else
            {
                return ComputeReceiverSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations);
            }
        }

        private Task<BitArray> ComputeSenderSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
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

            return _obliviousTransfer.SendAsync(channel, options, numberOfInvocations).ContinueWith(task => randomShares);
        }

        private Task<BitArray> ComputeReceiverSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            QuadrupleIndexArray selectionIndices = new QuadrupleIndexArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
                selectionIndices[i] = 2 * (byte)leftShares[i] + (byte)rightShares[i];

            return _obliviousTransfer.ReceiveAsync(channel, selectionIndices, numberOfInvocations);
        }
    }
}
