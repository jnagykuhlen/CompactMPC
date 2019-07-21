using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A stateless 1-out-of-4 Oblivious Transfer implementation.
    /// 
    /// Stateless here means that the OT implementation does not maintain state for each channel (i.e., pair of communicating parties)
    /// in-between invocations.
    /// </summary>
    public interface IStatelessFourChoicesObliviousTransfer
    {
        Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes);
        Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);
    }
}
