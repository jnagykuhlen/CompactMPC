using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.Protocol
{
    public interface IMultiplicativeSharing
    {
        Task<BitArray> ComputeMultiplicativeSharesAsync(IMultiPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
    }
}
