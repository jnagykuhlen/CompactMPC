using System.Threading.Tasks;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;

namespace CompactMPC.Protocol
{
    public interface ISecureComputation
    {
        Task<BitArray> EvaluateAsync(IAsyncBatchEvaluableCircuit evaluable, InputPartyMapping inputMapping, OutputPartyMapping outputMapping, BitArray localInputs);
        IMultiPartyNetworkSession MultiPartySession { get; }
    }
}
