using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// Commmon base implementation of the OT extension protocol and its random and correlated variants.
    /// 
    /// Subclasses for variants must implement the SendAsync function using the provided ReceiveQMatrix function.
    /// The SendAsync implementation must take care to increment the InvocationCounter by the corresponding number
    /// of invocations.
    /// 
    /// Subclasses must also implement the ReceiveMaskedOptionsAsync function which gets called
    /// from ReceiveAsync.
    /// 
    /// See <see cref="TwoChoicesExtendedObliviousTransferChannel" />, <see cref="TwoChoicesCorrelatedExtendedObliviousTransferChannel" /> and <see cref="TwoChoicesRandomExtendedObliviousTransferChannel" />
    /// </summary>
    /// <remarks>
    /// References: Yuval Ishai, Joe Kilian, Kobbi Nissim and Erez Petrank: Extending Oblivious Transfers Efficiently. 2003. https://link.springer.com/content/pdf/10.1007/978-3-540-45146-4_9.pdf
    /// and Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public abstract class TwoChoicesExtendedObliviousTransferChannelBase
    {
        private ITwoChoicesObliviousTransferChannel _baseOT;

        protected int SecurityParameter { get; private set; }
        protected RandomOracle RandomOracle { get; private set; }


        /// <summary>
        /// Internal encapsulation of the persistent state for the sender role.
        /// </summary>
        private struct SenderState
        {
            public IEnumerable<byte>[] SeededRandomOracles;
            public BitArray RandomChoices;
        };
        private SenderState _senderState;
        
        /// <summary>
        /// Internal encapsulation of the persistent state for the receiver role.
        /// </summary>
        private struct ReceiverState
        {
            public Pair<IEnumerable<byte>>[] SeededRandomOracles;
        }
        private ReceiverState _receiverState;

        // Accessors to sender and receiver state properties for subclasses
        protected IEnumerable<byte>[] SenderOracles { get { return _senderState.SeededRandomOracles; } }
        protected BitArray SenderChoices { get { return _senderState.RandomChoices.Clone(); } }

        protected Pair<IEnumerable<byte>>[] ReceiverOracles { get { return _receiverState.SeededRandomOracles; } }

        public IMessageChannel Channel { get { return _baseOT.Channel;  } }
        protected RandomNumberGenerator RandomNumberGenerator { get; private set; }

        public TwoChoicesExtendedObliviousTransferChannelBase(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
        {
            RandomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
            RandomOracle = new HashRandomOracle(cryptoContext.HashAlgorithm);
            SecurityParameter = securityParameter;
            _baseOT = baseOT;
            _senderState = new SenderState();
            _receiverState = new ReceiverState();
        }

        /// <summary>
        /// Performs k many 1-out-of-2 OTs on k bits for the sender, where k is the security parameter, using the base OT implementation.
        /// 
        /// These are subsequently expanded into m many 1oo2 OTs on arbitrarily long messages
        /// by the SendAsync method, where m is only bounded by the amount of secure randomness the random
        /// oracle implementation can produce.
        /// </summary>
        public async Task ExecuteSenderBaseOTAsync()
        {
            _senderState = new SenderState();
            _senderState.RandomChoices = RandomNumberGenerator.GetBits(SecurityParameter);

            // retrieve seeds for OT extension via _securityParameter many base OTs
            int requiredBytes = BitArray.RequiredBytes(SecurityParameter);
            byte[][] seeds = await _baseOT.ReceiveAsync(_senderState.RandomChoices, SecurityParameter, requiredBytes);
            Debug.Assert(seeds.Length == SecurityParameter);

            // initializing a random oracle based on each seed
            _senderState.SeededRandomOracles = new IEnumerable<byte>[SecurityParameter];
            for (int k = 0; k < SecurityParameter; ++k)
            {
                Debug.Assert(seeds[k].Length == requiredBytes);
                _senderState.SeededRandomOracles[k] = RandomOracle.Invoke(seeds[k]);
            }
        }

        /// <summary>
        /// Performs k many 1-out-of-2 OTs on k bits for the receiver, where k is the security parameter, using the base OT implementation.
        /// 
        /// These are subsequently expanded into m many 1oo2 OTs on arbitrarily long messages
        /// by the ReceiveAsync method, where m is only bounded by the amount of secure randomness the random
        /// oracle implementation can produce.
        /// </summary>
        public async Task ExecuteReceiverBaseOTAsync()
        {
            _receiverState = new ReceiverState();
            // generating _securityParameter many pairs of random seeds of length _securityParameter
            Pair<byte[]>[] seeds = new Pair<byte[]>[SecurityParameter];
            for (int k = 0; k < SecurityParameter; ++k)
            {
                seeds[k] = new Pair<byte[]>(
                    RandomNumberGenerator.GetBits(SecurityParameter).ToBytes(),
                    RandomNumberGenerator.GetBits(SecurityParameter).ToBytes()
                );
            }

            // base OTs as _sender_ with the seeds as inputs
            int requiredBytes = BitArray.RequiredBytes(SecurityParameter);
            Task sendTask = _baseOT.SendAsync(seeds, SecurityParameter, requiredBytes);

            // initializing a random oracle based on each seed
            _receiverState.SeededRandomOracles = new Pair<IEnumerable<byte>>[SecurityParameter];
            for (int k = 0; k < SecurityParameter; ++k)
            {
                _receiverState.SeededRandomOracles[k] = new Pair<IEnumerable<byte>>(
                    RandomOracle.Invoke(seeds[k][0]),
                    RandomOracle.Invoke(seeds[k][1])
                );
            };

            await sendTask;
        }

        public Task<byte[][]> ReceiveAsync(PairIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            return ReceiveAsync(BitArray.FromPairIndexArray(selectionIndices), numberOfInvocations, numberOfMessageBytes);
        }

        public async Task<byte[][]> ReceiveAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Amount of selection indices must match the specified number of invocations.", nameof(selectionIndices));

            if (_receiverState.SeededRandomOracles == null)
                await ExecuteReceiverBaseOTAsync();

            // note(lumip): we need a random bit matrix t as per the construction
            //  of Ishai et al. and we use the trick of Asharov et al. and populate it with the random
            //  expansion of the base OT seeds, i.e., t_k = H(seed_0). Since these are calculated later,
            //  we only set up the matrix structure here.
            BitMatrix tTransposed = new BitMatrix((uint)SecurityParameter, (uint)numberOfInvocations);

            // "perform" _securityParams many 1-out-of-2 OTs with numberOfInvocations bits message length
            // by extending the _securityParams many 1-out-of-2 OTs with _securityParams bits message length
            // (using the seeded random oracles)
            // the purpose of these OTs is to offer to the _sender_ either a column of t or the column xor'ed with
            // the selection bits of the receiver

            // todo(lumip): should try-catch in case random oracle runs out
            //  and ideally performs new base OTs and repeat
            int numberOfRandomBytes = BitArray.RequiredBytes(numberOfInvocations);
            byte[][][] sendBuffer = new byte[SecurityParameter][][];

            Parallel.For(0, SecurityParameter, k =>
            {
                BitArray tColumn = BitArray.FromBytes(
                    _receiverState.SeededRandomOracles[k][0].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );
                tTransposed.SetRow((uint)k, tColumn);

                BitArray mask = BitArray.FromBytes(
                    _receiverState.SeededRandomOracles[k][1].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );

                BitArray maskedSecondOption = tColumn ^ mask ^ selectionIndices;
                sendBuffer[k] = new byte[1][] { maskedSecondOption.ToBytes() };
            });

            await CommunicationTools.WriteOptionsAsync(Channel, sendBuffer, 1, SecurityParameter, numberOfRandomBytes);

            return await ReceiveMaskedOptionsAsync(selectionIndices, tTransposed, numberOfInvocations, numberOfMessageBytes);
        }

        /// <summary>
        /// Receives and processes the masked options from the sender and produces the final output.
        /// 
        /// Gets called from ReceiveAsync which performs the preprocessing steps (expanding the base OTs for the number of invocations, etc..)
        /// that are common for all OT extension variants. ReceiveMaskedOptionsAsync is then the part where different OT extension variants
        /// differ, so it must be implemented in specific subclasses.
        /// 
        /// Implementation of this method must not modify the invocation counter in the receiver state.
        /// </summary>
        /// <param name="selectionIndices">An array of selection bits, one for each invocation.</param>
        /// <param name="tTransposed">The random matrix T as specified by Ishai et al., transposed.</param>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <param name="numberOfMessageBytes">The length of each option/message in bytes.</param>
        /// <returns>An array of the received options/messages.</returns>
        protected abstract Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes);

        /// <summary>
        /// Receives and returns the matrix Q as specified in the OT extension protocol of Ishai et al.
        /// 
        /// This is involves the preprocessing (expanding the base OTs for the number of invocations, etc..)
        /// for all variants of the OT extension. This method should be called is the first step by
        /// subclasses implementing the OT extension variants.
        /// </summary>
        /// <param name="numberOfInvocations">The number of invocations/instances of OT.</param>
        /// <returns>The matrix Q of correlated randomness.</returns>
        protected async Task<BitMatrix> ReceiveQMatrix(int numberOfInvocations)
        {
            if (_senderState.SeededRandomOracles == null)
                await ExecuteSenderBaseOTAsync();

            int numberOfRandomBytes = BitArray.RequiredBytes(numberOfInvocations);

            BitMatrix q = new BitMatrix((uint)numberOfInvocations, (uint)SecurityParameter);

            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // todo(lumip): should try-catch in case random oracle runs out
            //  and ideally performs new base OTs and repeat
            byte[][][] qOTResult = await CommunicationTools.ReadOptionsAsync(Channel, 1, SecurityParameter, numberOfRandomBytes);
            Debug.Assert(qOTResult.Length == SecurityParameter);

            PairIndexArray randomChoiceInts = _senderState.RandomChoices.ToPairIndexArray();

            Parallel.For(0, SecurityParameter, k =>
            {
                Debug.Assert(qOTResult[k].Length == 1);
                Debug.Assert(qOTResult[k][0].Length == numberOfRandomBytes);

                BitArray mask = BitArray.FromBytes(
                    _senderState.SeededRandomOracles[k].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );
                int s = randomChoiceInts[k];

                BitArray qColumn = mask;
                if (s == 1)
                    qColumn.Xor(BitArray.FromBytes(qOTResult[k][0], numberOfInvocations));

                q.SetColumn((uint)k, qColumn);
            });
            return q;
        }
    }
}