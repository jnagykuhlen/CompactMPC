using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public abstract class PairwiseMultiplicativeSharing : IMultiplicativeSharing
{
    public async Task<BitArray> ComputeMultiplicativeSharesAsync(OrderedMultiPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations)
    {
        var pairwiseMultiplicativeShares = await session.PairwiseSendOrReceiveAsync(
            channel => ComputeSenderSharesAsync(channel, leftShares, rightShares, numberOfInvocations),
            channel => ComputeReceiverSharesAsync(channel, leftShares, rightShares, numberOfInvocations)
        );

        if (!IncludesLocalTerms || session.NumberOfParties.IsOdd)
            return leftShares.And(rightShares).Xor(pairwiseMultiplicativeShares);

        return BitArray.FromXor(pairwiseMultiplicativeShares);
    }

    protected abstract Task<BitArray> ComputeSenderSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
    protected abstract Task<BitArray> ComputeReceiverSharesAsync(IMessageChannel channel, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
    protected abstract bool IncludesLocalTerms { get; }
}
