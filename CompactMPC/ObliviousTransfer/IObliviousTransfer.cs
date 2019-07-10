using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Common interface for 1-out-of-4 single bit oblivious transfer implementations.
    /// 
    /// For a variant with arbitrary bit length for messages/options, see <see cref="IGeneralizedObliviousTransfer"/>.
    /// </summary>
    public interface IObliviousTransfer
    {
        /// <summary>
        /// Supplies the four options from the sender to the oblivious transfer.
        /// </summary>
        /// <param name="channel">The network message channel to the receiver.</param>
        /// <param name="options">Array containing the four options supplied to 1-out-of-4 OT by the sender for each invocation.</param>
        /// <param name="numberOfInvocations">The number of OT invocations in the transmission.</param>
        /// <returns>An asynchronous task which performs the server side of the oblivious transfer.</returns>
        /// <remarks>To increase efficiency, several invocations of OT can be batched into one transmission.</remarks>
        Task SendAsync(IMessageChannel channel, BitQuadrupleArray options, int numberOfInvocations);

        /// <summary>
        /// Supplies the selection index from the client side to the oblivious transfer and returns the corresponding option from the server.
        /// </summary>
        /// <param name="channel">The network message channel to the sender.</param>
        /// <param name="selectionIndices">Array containing the selection index supplied to 1-out-of-4 OT by the client for each invocation.</param>
        /// <param name="numberOfInvocations">The number of OT invocations in the transmission.</param>
        /// <returns>An asynchronous task performing the client side of the oblivious transfer and returning the array containing
        /// the options as selected by the client retrieved from the server.</returns>
        /// <remarks>To increase efficiency, several invocations of OT can be batched into one transmission.</remarks>
        Task<BitArray> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations);
    }
}
