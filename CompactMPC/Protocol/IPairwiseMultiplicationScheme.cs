using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.Protocol
{
    public interface IPairwiseMultiplicationScheme
    {
        Task<BitArray> ComputeMultiplicationSharesAsync(ITwoPartyNetworkSession session, BitArray leftShares, BitArray rightShares, int numberOfInvocations);
    }
}
