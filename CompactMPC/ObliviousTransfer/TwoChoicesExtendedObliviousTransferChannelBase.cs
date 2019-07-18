using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using CompactMPC.Networking;
using CompactMPC.Buffers;

namespace CompactMPC.ObliviousTransfer
{
    public abstract class TwoChoicesExtendedObliviousTransferChannelBase
    {
        protected int _securityParameter;
        protected ITwoChoicesObliviousTransferChannel _baseOT;
        protected RandomOracle _randomOracle;
        
        protected struct SenderState
        {
            public IEnumerable<byte>[] SeededRandomOracles;
            public BitArray RandomChoices;
            public uint InvocationCounter;
        };
        protected SenderState _senderState;

        protected struct ReceiverState
        {
            public Pair<IEnumerable<byte>>[] SeededRandomOracles;
            public uint InvocationCounter;
        }
        protected ReceiverState _receiverState;

        public IMessageChannel Channel { get { return _baseOT.Channel;  } }
        protected RandomNumberGenerator RandomNumberGenerator { get; private set; }

        public TwoChoicesExtendedObliviousTransferChannelBase(ITwoChoicesObliviousTransferChannel baseOT, int securityParameter, CryptoContext cryptoContext)
        {
            RandomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
            _randomOracle = new HashRandomOracle(cryptoContext.HashAlgorithm);
            _securityParameter = securityParameter;
            _baseOT = baseOT;
            _senderState = new SenderState();
            _receiverState = new ReceiverState();
        }

        public async Task ExecuteSenderBaseOTAsync()
        {
            _senderState = new SenderState();
            _senderState.RandomChoices = RandomNumberGenerator.GetBits(_securityParameter);

            // retrieve seeds for OT extension via _securityParameter many base OTs
            int requiredBytes = BitArray.RequiredBytes(_securityParameter);
            byte[][] seeds = await _baseOT.ReceiveAsync(_senderState.RandomChoices, _securityParameter, requiredBytes);
            _senderState.InvocationCounter = 0;

            // initializing a random oracle based on each seed
            _senderState.SeededRandomOracles = new IEnumerable<byte>[_securityParameter];
            for (int k = 0; k < _securityParameter; ++k)
            {
                _senderState.SeededRandomOracles[k] = _randomOracle.Invoke(seeds[k]);
            }
        }

        public async Task ExecuteReceiverBaseOTAsync()
        {
            _receiverState = new ReceiverState();
            // generating _securityParameter many pairs of random seeds of length _securityParameter
            Pair<byte[]>[] seeds = new Pair<byte[]>[_securityParameter];
            for (int k = 0; k < _securityParameter; ++k)
            {
                seeds[k] = new Pair<byte[]>(
                    RandomNumberGenerator.GetBits(_securityParameter).ToBytes(),
                    RandomNumberGenerator.GetBits(_securityParameter).ToBytes()
                );
            }

            // base OTs as _sender_ with the seeds as inputs
            int requiredBytes = BitArray.RequiredBytes(_securityParameter);
            Task sendTask = _baseOT.SendAsync(seeds, _securityParameter, requiredBytes);

            // initializing a random oracle based on each seed
            _receiverState.SeededRandomOracles = new Pair<IEnumerable<byte>>[_securityParameter];
            for (int k = 0; k < _securityParameter; ++k)
            {
                _receiverState.SeededRandomOracles[k] = new Pair<IEnumerable<byte>>(
                    _randomOracle.Invoke(seeds[k][0]),
                    _randomOracle.Invoke(seeds[k][1])
                );
            };
            _receiverState.InvocationCounter = 0;

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
            BitMatrix tTransposed = new BitMatrix((uint)_securityParameter, (uint)numberOfInvocations);

            // perform _securityParams many 1-out-of-2 OTs with numberOfInvocations bits message length
            // by extending the _securityParams many 1-out-of-2 OTs with _securityParams bits message length
            // (using the seeded random oracles)
            // the purpose of these OTs is to offer to the _sender_ either a column of t or the column xor'ed with
            // the selection bits of the receiver

            // todo(lumip): should try-catch in case random oracle runs out
            //  and ideally performs new base OTs and repeat
            int numberOfRandomBytes = BitArray.RequiredBytes(numberOfInvocations);
            byte[][][] sendBuffer = new byte[_securityParameter][][];
            Parallel.For(0, _securityParameter, k =>
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

            await CommunicationTools.WriteOptionsAsync(Channel, sendBuffer, 1, _securityParameter, numberOfRandomBytes);

            byte[][] results = await ReceiveMaskedOptionsAsync(selectionIndices, tTransposed, numberOfInvocations, numberOfMessageBytes);
            
            _receiverState.InvocationCounter += (uint)numberOfInvocations;
            return results;
        }

        protected abstract Task<byte[][]> ReceiveMaskedOptionsAsync(BitArray selectionIndices, BitMatrix tTransposed, int numberOfInvocations, int numberOfMessageBytes);

        protected async Task<BitMatrix> ReceiveQMatrix(int numberOfInvocations)
        {
            if (_senderState.SeededRandomOracles == null)
                await ExecuteSenderBaseOTAsync();

            int numberOfRandomBytes = BitArray.RequiredBytes(numberOfInvocations);

            BitMatrix q = new BitMatrix((uint)numberOfInvocations, (uint)_securityParameter);

            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // todo(lumip): should try-catch in case random oracle runs out
            //  and ideally performs new base OTs and repeat
            byte[][][] qOTResult = await CommunicationTools.ReadOptionsAsync(Channel, 1, _securityParameter, numberOfRandomBytes);
            Debug.Assert(qOTResult.Length == _securityParameter);

            PairIndexArray randomChoiceInts = _senderState.RandomChoices.ToPairIndexArray();

            Parallel.For(0, _securityParameter, k =>
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