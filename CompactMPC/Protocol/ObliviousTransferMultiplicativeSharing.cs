using System.Security.Cryptography;
using System.Threading.Tasks;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;

namespace CompactMPC.Protocol
{
    public class ObliviousTransferMultiplicativeSharing : PairwiseMultiplicativeSharing
    {
        private readonly IObliviousTransfer _obliviousTransfer;
        private readonly RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferMultiplicativeSharing(IObliviousTransfer obliviousTransfer, CryptoContext cryptoContext)
        {
            _obliviousTransfer = obliviousTransfer;
            _randomNumberGenerator = new ThreadSafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        protected override Task<BitArray> ComputePairwiseMultiplicativeSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // Given local shares x1, y1
            // compute share of x1 * y2 + x2 * y1
            return session.RemoteParty.Id < session.LocalParty.Id ?
                ComputeSenderSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations) :
                ComputeReceiverSharesAsync(session.Channel, leftShares, rightShares, numberOfInvocations);
        }

        private async Task<BitArray> ComputeSenderSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
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

        private Task<BitArray> ComputeReceiverSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            QuadrupleIndexArray selectionIndices = new QuadrupleIndexArray(numberOfInvocations);
            for (int i = 0; i < numberOfInvocations; ++i)
                selectionIndices[i] = 2 * (byte)leftShares[i] + (byte)rightShares[i];

            return _obliviousTransfer.ReceiveAsync(channel, selectionIndices, numberOfInvocations);
        }

        protected override bool IncludesLocalTerms
        {
            get
            {
                return false;
            }
        }
    }
}
