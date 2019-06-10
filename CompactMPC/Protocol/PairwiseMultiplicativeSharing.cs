using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.Protocol
{
    public abstract class PairwiseMultiplicativeSharing : IMultiplicativeSharing
    {
        public async Task<BitArray> ComputeMultiplicativeSharesAsync(IMultiPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
        {
            BitArray localMultiplicationShares = new BitArray(numberOfInvocations);
            if (!IncludesLocalTerms || session.HasEvenNumberOfRemoteParties())
                localMultiplicationShares = leftShares & rightShares;

            Task<BitArray>[] pairwiseMultiplicativeSharingTasks = new Task<BitArray>[session.NumberOfParties];
            pairwiseMultiplicativeSharingTasks[session.LocalParty.Id] = Task.FromResult(localMultiplicationShares);

            Parallel.ForEach(session.RemotePartySessions, pairwiseSession =>
            {
                pairwiseMultiplicativeSharingTasks[pairwiseSession.RemoteParty.Id] = ComputePairwiseMultiplicativeSharesAsync(
                    pairwiseSession,
                    leftShares,
                    rightShares,
                    numberOfInvocations
                );
            });

            BitArray[] pairwiseMultiplicativeShares = await Task.WhenAll(pairwiseMultiplicativeSharingTasks).ConfigureAwait(false);
            return pairwiseMultiplicativeShares.Aggregate((left, right) => left ^ right);
        }

        protected abstract Task<BitArray> ComputePairwiseMultiplicativeSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
        protected abstract bool IncludesLocalTerms { get; }
    }
}
