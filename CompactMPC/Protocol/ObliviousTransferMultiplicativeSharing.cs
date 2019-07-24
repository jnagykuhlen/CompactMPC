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
    public class ObliviousTransferMultiplicativeSharing : PairwiseMultiplicativeSharing
    {
        private IObliviousTransferProvider<ITwoChoicesCorrelatedBitObliviousTransferChannel> _obliviousTransferProvider;
        private RandomNumberGenerator _randomNumberGenerator;

        public ObliviousTransferMultiplicativeSharing(IObliviousTransferProvider<ITwoChoicesCorrelatedBitObliviousTransferChannel> obliviousTransferProvider, CryptoContext cryptoContext)
        {
            _obliviousTransferProvider = obliviousTransferProvider;
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
        }

        protected override Task<BitArray> ComputePairwiseMultiplicativeSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // Given local shares x1, y1
            // compute share of x1 * y2 + x2 * y1
            if (session.RemoteParty.Id < session.LocalParty.Id)
            {
                return ComputeSenderSharesAsync(_obliviousTransferProvider.CreateChannel(session.Channel), leftShares, rightShares, numberOfInvocations);
            }
            else
            {
                return ComputeReceiverSharesAsync(_obliviousTransferProvider.CreateChannel(session.Channel), leftShares, rightShares, numberOfInvocations);
            }
        }

        private async Task<BitArray> ComputeSenderSharesAsync(ITwoChoicesCorrelatedBitObliviousTransferChannel ot, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // todo(lumip): can't currently execute receive and send in parallel, even though they are independent, due to limitations of the message channels
            Pair<Bit>[] correlatedShared = await ot.SendAsync(leftShares, numberOfInvocations);
            BitArray c = await ot.ReceiveAsync(rightShares, numberOfInvocations);

            BitArray r = new BitArray(correlatedShared.Select(pair => pair[0]).ToArray());
            return r ^ c;
        }

        private async Task<BitArray> ComputeReceiverSharesAsync(ITwoChoicesCorrelatedBitObliviousTransferChannel ot, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            // todo(lumip): can't currently execute receive and send in parallel, even though they are independent, due to limitations of the message channels
            BitArray c = await ot.ReceiveAsync(rightShares, numberOfInvocations);
            Pair<Bit>[] correlatedShared = await ot.SendAsync(leftShares, numberOfInvocations);

            BitArray r = new BitArray(correlatedShared.Select(pair => pair[0]).ToArray());
            return r ^ c;
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
