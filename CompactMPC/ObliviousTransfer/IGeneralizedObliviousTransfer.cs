using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IGeneralizedObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
