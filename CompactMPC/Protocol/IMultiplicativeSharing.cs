using System.Threading.Tasks;
using CompactMPC.Networking;
using CompactMPC.Protocol.Internal;

namespace CompactMPC.Protocol;

public interface IMultiplicativeSharing
{
    Task<BitArray> ComputeMultiplicativeSharesAsync(OrderedMultiPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
}
