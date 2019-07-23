using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    public interface ITwoChoicesRandomBitObliviousTransferChannel
    {
        /// <summary>
        /// Sends one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are generated independently at random and output to the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of pairs of random options.</returns>
        Task<Pair<Bit>[]> SendAsync(int numberOfInvocations);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are generated independently at random.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of selection bits, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<BitArray> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are generated independently at random.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of pair indices, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<BitArray> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations);

        /// <summary>
        /// The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.
        /// </summary>
        IMessageChannel Channel { get; }
    }
}
