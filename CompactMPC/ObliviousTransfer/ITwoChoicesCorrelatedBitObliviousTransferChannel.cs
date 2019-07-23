using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-2 Correlated Bit Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo2-C-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// 
    /// The C-OT functionality receives a correlation bit ∆ and chooses the sender’s inputs uniformly under the constraint that their XOR equals ∆. [Asharov et al.]
    /// </summary>
    /// <remarks>
    /// Reference: Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public interface ITwoChoicesCorrelatedBitObliviousTransferChannel
    {
        /// <summary>
        /// Sends one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions is explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="correlationBits">An array of correlation bits, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of pairs of random options correlated by the given correlation strings.</returns>
        Task<Pair<Bit>[]> SendAsync(BitArray correlationBits, int numberOfInvocations);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions is explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of selection bits, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<BitArray> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions is explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of pair indices, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<BitArray> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations);
    }
}
