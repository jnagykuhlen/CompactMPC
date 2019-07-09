using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Linq;
using System.Diagnostics;

using CompactMPC.Networking;
using CompactMPC.Buffers;

namespace CompactMPC.ObliviousTransfer
{
    internal class CommunicationTools
    {
        public static Task WriteOptionsAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] messageBuffer = new byte[numberOfOptions * numberOfMessageBytes * numberOfInvocations];
            Parallel.For(0, numberOfInvocations, i =>
            {
                for (int j = 0; j < numberOfOptions; ++j)
                {
                    Buffer.BlockCopy(
                        options[i][j],
                        0,
                        messageBuffer,
                        (numberOfOptions * numberOfMessageBytes) * i + j * numberOfMessageBytes,
                        numberOfMessageBytes
                    );
                }
            });

            return channel.WriteMessageAsync(messageBuffer);
        }

        public static async Task<byte[][][]> ReadOptionsAsync(IMessageChannel channel, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            byte[] messageBuffer = await channel.ReadMessageAsync();
            byte[][][] result = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                result[i] = new byte[numberOfOptions][];
                for (int j = 0; j < numberOfOptions; ++j)
                {
                    result[i][j] = new byte[numberOfMessageBytes];
                    Buffer.BlockCopy(
                        messageBuffer,
                        (numberOfOptions * numberOfMessageBytes) * i + j * numberOfMessageBytes,
                        result[i][j],
                        0,
                        numberOfMessageBytes
                    );
                }
            });
            return result;
        }
    }

    internal class ExtendedObliviousTransferReceiver
    {
        private int _securityParameter;
        private IGeneralizedObliviousTransfer _baseOT;
        private RandomOracle _randomOracle;
        private RandomNumberGenerator _randomNumberGenerator;
        private IMessageChannel _channel;

        private Pair<IEnumerable<byte>>[] _seededRandomOracles;
        private uint _invocationCounter;

        public ExtendedObliviousTransferReceiver(IGeneralizedObliviousTransfer baseOT, int securityParameter, IMessageChannel channel, CryptoContext cryptoContext)
        {
            _baseOT = baseOT;
            _securityParameter = securityParameter;
            _randomOracle = new HashRandomOracle(cryptoContext.HashAlgorithm);
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
            _channel = channel;
            _seededRandomOracles = null;
            _invocationCounter = 0;
        }

        /// <summary>
        /// Performs k many 1-out-of-2 OTs on k bits, where k is a security parameter.
        /// 
        /// These so called base OTs are subsequently expanded into m many 1oo2 OTs on arbitrarily long messages
        /// by the ExecuteAsync methods, where m is only bounded by the amount of secure randomness the random
        /// oracle implementation can produce.
        /// </summary>
        public async Task ExecuteBaseOTAsync()
        {
            // generating _securityParameter many pairs of random seeds of length _securityParameter
            byte[][][] seeds = new byte[_securityParameter][][];
            Parallel.For(0, _securityParameter, k =>
            {
                seeds[k] = new byte[][] {
                    _randomNumberGenerator.GetBits(_securityParameter).ToBytes(),
                    _randomNumberGenerator.GetBits(_securityParameter).ToBytes()
                };
            });

            // base OTs as _sender_ with the seeds as inputs
            int requiredBytes = BitArray.RequiredBytes(_securityParameter);
            Task sendTask = _baseOT.SendAsync(_channel, seeds, 2, _securityParameter, requiredBytes);

            // initializing a random oracle based on each seed
            _seededRandomOracles = new Pair<IEnumerable<byte>>[_securityParameter];
            Parallel.For(0, _securityParameter, k =>
            {
                _seededRandomOracles[k] = new Pair<IEnumerable<byte>>(
                    _randomOracle.Invoke(seeds[k][0]),
                    _randomOracle.Invoke(seeds[k][1])
                );
            });
            _invocationCounter = 0;

            await sendTask;
        }

        public async Task<byte[][]> ExecuteAsync(BitArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            Debug.Assert(selectionIndices.Length == numberOfInvocations);

            if (_seededRandomOracles == null)
                await ExecuteBaseOTAsync();

            // draw random bit matrix t
            BitMatrix tTransposed = new BitMatrix((uint)_securityParameter, (uint)numberOfInvocations);

            Console.WriteLine("[Receiver] selection indices {0}", selectionIndices.ToBinaryString());

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
                    _seededRandomOracles[k][0].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );
                tTransposed.SetRow((uint)k, tColumn);

                BitArray mask = BitArray.FromBytes(
                    _seededRandomOracles[k][1].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );

                BitArray maskedSecondOption = tColumn ^ mask ^ selectionIndices;
                sendBuffer[k] = new byte[1][] { maskedSecondOption.ToBytes() };
            });

            await CommunicationTools.WriteOptionsAsync(_channel, sendBuffer, 1, _securityParameter, numberOfRandomBytes);

            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // retrieve the masked options from the sender and unmask the one indicated by the
            //  corresponding selection indices
            byte[][][] maskedOptions = await CommunicationTools.ReadOptionsAsync(_channel, 2, numberOfInvocations, numberOfMessageBytes);

            byte[][] results = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _invocationCounter + (uint)i;
                int s = Convert.ToInt32(selectionIndices[i].Value);

                byte[] query = BufferBuilder.From(tTransposed.GetColumn((uint)i).ToBytes()).With((int)invocationIndex).With(s).Create();
                results[i] = _randomOracle.Mask(maskedOptions[i][s], query);
            });
            _invocationCounter += (uint)numberOfInvocations;
            return results;
        }

    }

    internal class ExtendedObliviousTransferSender
    {
        private int _securityParameter;
        private IGeneralizedObliviousTransfer _baseOT;
        private RandomOracle _randomOracle;
        private RandomNumberGenerator _randomNumberGenerator;
        private IMessageChannel _channel;

        private IEnumerable<byte>[] _seededRandomOracles;
        private BitArray _randomChoices;
        private uint _invocationCounter;

        /// <summary>
        /// Performs k many 1-out-of-2 OTs on k bits, where k is a security parameter.
        /// 
        /// These so called base OTs are subsequently expanded into m many 1oo2 OTs on arbitrarily long messages
        /// by the ExecuteAsync methods, where m is only bounded by the amount of secure randomness the random
        /// oracle implementation can produce.
        /// </summary>
        public ExtendedObliviousTransferSender(IGeneralizedObliviousTransfer baseOT, int securityParameter, IMessageChannel channel, CryptoContext cryptoContext)
        {
            _baseOT = baseOT;
            _securityParameter = securityParameter;
            _randomOracle = new HashRandomOracle(cryptoContext.HashAlgorithm);
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
            _channel = channel;

            _seededRandomOracles = null;
            _randomChoices = null;
            _invocationCounter = 0;
        }

        public async Task ExecuteBaseOTAsync()
        {
            _randomChoices = _randomNumberGenerator.GetBits(_securityParameter);
            int[] s = _randomChoices.Select(b => Convert.ToInt32(b.Value)).ToArray();

            // retrieve seeds for OT extension via _securityParameter many base OTs
            int requiredBytes = BitArray.RequiredBytes(_securityParameter);
            byte[][] seeds = await _baseOT.ReceiveAsync(_channel, s, 2, _securityParameter, requiredBytes);
            _invocationCounter = 0;
            
            // initializing a random oracle based on each seed
            _seededRandomOracles = new IEnumerable<byte>[_securityParameter];
            Parallel.For(0, _securityParameter, k =>
            {
                _seededRandomOracles[k] = _randomOracle.Invoke(seeds[k]);
            });
        }


        public async Task ExecuteAsync(Pair<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            Debug.Assert(options.Length == numberOfInvocations);

            if (_seededRandomOracles == null)
                await ExecuteBaseOTAsync();

            int numberOfRandomBytes = BitArray.RequiredBytes(numberOfInvocations);

            BitMatrix q = new BitMatrix((uint)numberOfInvocations, (uint)_securityParameter);

            // note(lumip): could precompute the masks for the response to have only cheap Xor after
            //  waiting for the message, but not sure if it really is a significant performance issue

            // todo(lumip): should try-catch in case random oracle runs out
            //  and ideally performs new base OTs and repeat
            byte[][][] qOTResult = await CommunicationTools.ReadOptionsAsync(_channel, 1, _securityParameter, numberOfRandomBytes);

            Parallel.For(0, _securityParameter, k =>
            {
                BitArray mask = BitArray.FromBytes(
                    _seededRandomOracles[k].Take(numberOfRandomBytes).ToArray(),
                    numberOfInvocations
                );
                int s = Convert.ToInt32(_randomChoices[k].Value);

                BitArray qColumn = mask;
                if (s == 1)
                    qColumn.Xor(BitArray.FromBytes(qOTResult[k][0], numberOfInvocations));

                q.SetColumn((uint)k, qColumn);
            });

            byte[][][] maskedOptions = new byte[numberOfInvocations][][];
            Parallel.For(0, numberOfInvocations, i =>
            {
                uint invocationIndex = _invocationCounter + (uint)i;
                maskedOptions[i] = new byte[2][];
                BitArray qRow = q.GetRow((uint)i);
                for (int j = 0; j < 2; ++j)
                {
                    Debug.Assert(options[i][j].Length == numberOfMessageBytes);
                    if (j == 1)
                        qRow.Xor(_randomChoices);
                    Console.WriteLine("[Sender] qRow {0} {1}: {2}", i, j, qRow.ToBinaryString());
                    byte[] query = BufferBuilder.From(qRow.ToBytes()).With((int)invocationIndex).With(j).Create();
                    maskedOptions[i][j] = _randomOracle.Mask(options[i][j], query);
                }
            });

            await CommunicationTools.WriteOptionsAsync(_channel, maskedOptions, 2, numberOfInvocations, numberOfMessageBytes);
        }

    }

    /// <summary>
    /// Implementation of the OT extension protocol that enables extending a small number of expensive oblivious transfer
    /// invocation (so called "base OTs") into a larger number of usable oblivous transfer invocations using only cheap
    /// symmetric cryptography via a random oracle instantiation.
    /// </summary>
    /// <remarks>
    /// References: Yuval Ishai, Joe Kilian, Kobbi Nissim and Erez Petrank: Extending Oblivious Transfers Efficiently. 2003. https://link.springer.com/content/pdf/10.1007/978-3-540-45146-4_9.pdf
    /// and Gilad Asharov, Yehuda Lindell, Thomas Schneider and Michael Zohner: More Efficient Oblivious Transfer and Extensions for Faster Secure Computation. 2013. Section 5.3. https://thomaschneider.de/papers/ALSZ13.pdf
    /// </remarks>
    public class ExtendedObliviousTransfer : GeneralizedObliviousTransfer
    {
        // todo: consider optimizations due to Schneider et al. https://thomaschneider.de/papers/ALSZ13.pdf section 5.3
        private IMessageChannel _channel; // only used to check sanity of arguments for receive/send
        private ExtendedObliviousTransferSender _senderBehavior;
        private ExtendedObliviousTransferReceiver _receiverBehavior;

        public ExtendedObliviousTransfer(IGeneralizedObliviousTransfer baseOT, int securityParameter, IMessageChannel channel, CryptoContext cryptoContext)
        {
            _channel = channel;
            _senderBehavior = new ExtendedObliviousTransferSender(baseOT, securityParameter, channel, cryptoContext);
            _receiverBehavior = new ExtendedObliviousTransferReceiver(baseOT, securityParameter, channel, cryptoContext);
        }

        protected override async Task<byte[][]> GeneralizedReceiveAsync(IMessageChannel channel, int[] selectionIndices, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: OT extension doesn't fit the current interface properly. refactor interfaces and then remove the following assertions!
            Debug.Assert(numberOfOptions == 2); // the OT extension protocol is defined for 1-out-of-2 OT only
            Debug.Assert(channel == _channel);  // the OT extension protocol is only useful if it can maintain state (to extend large number of subsequent OTs after some base OTs)

            BitArray selectionBits = new BitArray(
                selectionIndices.Select(s => new Bit(Convert.ToBoolean(s))).ToArray()
            );
            return await _receiverBehavior.ExecuteAsync(selectionBits, numberOfInvocations, numberOfMessageBytes);
        }

        protected override async Task GeneralizedSendAsync(IMessageChannel channel, byte[][][] options, int numberOfOptions, int numberOfInvocations, int numberOfMessageBytes)
        {
            // todo: OT extension doesn't fit the current interface properly. refactor interfaces and then remove the following assertions!
            Debug.Assert(numberOfOptions == 2); // the OT extension protocol is defined for 1-out-of-2 OT only
            Debug.Assert(channel == _channel);  // the OT extension protocol is only useful if it can maintain state (to extend large number of subsequent OTs after some base OTs)

            Pair<byte[]>[] optionPairs = options.Select(pair => new Pair<byte[]>(pair)).ToArray();

            await _senderBehavior.ExecuteAsync(optionPairs, numberOfInvocations, numberOfMessageBytes);
        }
    }
}
