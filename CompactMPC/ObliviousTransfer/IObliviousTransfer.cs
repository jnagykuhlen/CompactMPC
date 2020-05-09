using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations);
        Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations);
    }
}
