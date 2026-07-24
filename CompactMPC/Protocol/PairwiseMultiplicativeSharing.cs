using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public abstract class PairwiseMultiplicativeSharing : IMultiplicativeSharing
{
    public async Task<BitArray> ComputeMultiplicativeSharesAsync(OrderedMultiPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
    {
        var pairwiseMultiplicativeShares = await Task.WhenAll(
            session.RemotePartySessions
                .AsParallel()
                .Select(pairwiseSession =>
                    ComputePairwiseMultiplicativeSharesAsync(
                        pairwiseSession,
                        leftShares,
                        rightShares,
                        numberOfInvocations
                    )
                )
        );

        if (!IncludesLocalTerms || session.HasOddNumberOfParties)
            return leftShares.And(rightShares).Xor(pairwiseMultiplicativeShares);

        return BitArray.FromXor(pairwiseMultiplicativeShares);
    }

    protected abstract Task<BitArray> ComputePairwiseMultiplicativeSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
    protected abstract bool IncludesLocalTerms { get; }
}
