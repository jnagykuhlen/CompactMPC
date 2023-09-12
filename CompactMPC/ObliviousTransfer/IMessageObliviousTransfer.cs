using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface IMessageObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<Message[]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
