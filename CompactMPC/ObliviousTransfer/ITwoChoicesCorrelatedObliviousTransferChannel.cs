using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// A 1-out-of-2 Correlated Oblivious Transfer channel implementation.
    /// 
    /// Provides 1oo2-C-OT on a given channel (i.e., pair of parties) and may maintain
    /// channel-specific protocol state in-between invocations.
    /// 
    /// The C-OT functionality receives a correlation string ∆ and chooses the sender’s inputs uniformly under the constraint that their XOR equals ∆. [Asharov et al.]
    /// </summary>
    /// <remarks>
    /// Reference: Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public interface ITwoChoicesCorrelatedObliviousTransferChannel
    {
        /// <summary>
        /// Sends one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="correlationStrings">An array of correlation strings, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <param name="numberOfMessageBytes">The length of each option/message in bytes.</param>
        /// <returns>An array of pairs of random options correlated by the given correlation strings.</returns>
        Task<Pair<byte[]>[]> SendAsync(byte[][] correlationStrings, int numberOfInvocations, int numberOfMessageBytes);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of selection bits, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <param name="numberOfMessageBytes">The length of each option/message in bytes.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        /// <summary>
        /// Receives one out of two options to the receiver. Which one is chosen by the receiver without the sender learning that. 
        /// The options are essentially random but correlated to each other by a correlation string provided by the sender.
        /// 
        /// Several invocations can be batched into one function call, indicated by the numberOfInvocations parameter. Each option
        /// for each invocation has to be of the specified length.
        /// 
        /// The number of invoactions and message length are explicitely passed as parameters to enforce that the options are given correctly.
        /// </summary>
        /// <param name="selectionIndices">An array of pair indices, one for each invocation.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <param name="numberOfMessageBytes">The length of each option/message in bytes.</param>
        /// <returns>An array of the received options/messages.</returns>
        Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes);

        /// <summary>
        /// The network channel the OT operates on, uniquely identifying the pair of parties involved in the OT.
        /// </summary>
        IMessageChannel Channel { get; }
    }
}
