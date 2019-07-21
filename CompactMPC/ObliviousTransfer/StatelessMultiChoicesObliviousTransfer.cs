using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Common base class for stateless 1-out-of-N Oblivious Transfer implementations.
    /// 
    /// Performs argument sanity checks and implements the two and four choices/options interfaces based on the multi choice variant
    /// that is implemented by subclasses. These should provide overrides for the static SendAsyncInternal and ReceiveAsyncInternal
    /// methods, which are invoked by SendAsync and ReceiveAsync after the arguments have been checked for consistency.
    /// </summary>
    public abstract class StatelessMultiChoicesObliviousTransfer : IStatelessMultiChoicesObliviousTransfer, IStatelessFourChoicesObliviousTransfer, IStatelessTwoChoicesObliviousTransfer
    {
        protected abstract Task SendAsyncInternal(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);
        protected abstract Task<byte[][]> ReceiveAsyncInternal(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes);

        public Task SendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Amount of provided option pairs must match the specified number of invocations.", nameof(options));

            for (int j = 0; j < options.Length; ++j)
            {
                if (options[j].Length != numberOfOptions)
                    throw new ArgumentException("At least one per-invocation options tuple does not provide the specified number of options.", nameof(options));

                foreach (byte[] message in options[j])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("The length of of at least one option does not match the specified message length.", nameof(options));
                }
            }

            return SendAsyncInternal(channel, options, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Amount of selection indices must match the specified number of invocations.", nameof(selectionIndices));

            for (int i = 0; i < selectionIndices.Length; ++i)
            {
                if (selectionIndices[i] >= numberOfOptions)
                    throw new ArgumentException("At least one selection index exceeds the number of available options.", nameof(numberOfOptions));
            }

            return ReceiveAsyncInternal(channel, selectionIndices, numberOfOptions, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(IMessageChannel channel, Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return SendAsync(channel, options.Select(pair => pair.ToArray()).ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToPairIndexArray(), numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToArray(), 2, numberOfInvocations, numberOfMessageBytes);
        }

        public Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            return SendAsync(channel, options.Select(quadruple => quadruple.ToArray()).ToArray(), 4, numberOfInvocations, numberOfMessageBytes);
        }

        public Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(channel, selectionIndices.ToArray(), 4, numberOfInvocations, numberOfMessageBytes);
        }

    }
}
