using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using CompactMPC.Buffers;
using CompactMPC.Cryptography;
using CompactMPC.Networking;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// 1-out-of-4 Oblivious Transfer implementation following a protocol by Naor and Pinkas.
    /// </summary>
    /// <remarks>
    /// Reference: Moni Naor and Benny Pinkas: Efficient oblivious transfer protocols 2001. https://dl.acm.org/citation.cfm?id=365502
    /// Further implementation details: Seung Geol Choi et al.: Secure Multi-Party Computation of Boolean Circuits with Applications
    /// to Privacy in On-Line Marketplaces. https://link.springer.com/chapter/10.1007/978-3-642-27954-6_26
    /// </remarks>
    public class NaorPinkasObliviousTransfer : ObliviousTransfer
    {
        private readonly SecurityParameters _parameters;
        private readonly RandomOracle _randomOracle;

        public NaorPinkasObliviousTransfer(SecurityParameters parameters)
        {
            _parameters = parameters;
            _randomOracle = new HashRandomOracle();
#if DEBUG
            Console.WriteLine("Security parameters:");
            Console.WriteLine("p = {0}", _parameters.P);
            Console.WriteLine("q = {0}", _parameters.Q);
            Console.WriteLine("g = {0}", _parameters.G);
            Console.WriteLine("group element size = {0} bytes", _parameters.GroupElementSize);
            Console.WriteLine("exponent size = {0} bytes", _parameters.ExponentSize);
#endif
        }

        protected override async Task InternalSendAsync(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {   
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            
            Quadruple<BigInteger> listOfCs = new Quadruple<BigInteger>();
            Quadruple<BigInteger> listOfExponents = new Quadruple<BigInteger>();

            Parallel.For(0, 4, i =>
            {
                listOfCs[i] = GenerateGroupElement(out BigInteger exponent);
                listOfExponents[i] = exponent;
            });

            BigInteger alpha = listOfExponents[0];

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Generating c took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Task writeCsTask = WriteGroupElements(channel, listOfCs);
            Task<BigInteger[]> readDsTask = ReadGroupElements(channel, numberOfInvocations);

            Quadruple<BigInteger> listOfExponentiatedCs = new Quadruple<BigInteger>();
            Parallel.For(1, 4, i =>
            {
                listOfExponentiatedCs[i] = BigInteger.ModPow(listOfCs[i], alpha, _parameters.P);
            });

            await Task.WhenAll(writeCsTask, readDsTask);
            BigInteger[] listOfDs = readDsTask.Result;

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Precomputing exponentiations, sending c and reading d took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Quadruple<Message>[] maskedOptions = new Quadruple<Message>[numberOfInvocations];
            Parallel.For(0, numberOfInvocations, j =>
            {
                maskedOptions[j] = new Quadruple<Message>();
                BigInteger exponentiatedD = BigInteger.ModPow(listOfDs[j], alpha, _parameters.P);
                BigInteger inverseExponentiatedD = Invert(exponentiatedD);

                Parallel.For(0, 4, i =>
                {
                    BigInteger e = exponentiatedD;
                    if (i > 0)
                        e = (listOfExponentiatedCs[i] * inverseExponentiatedD) % _parameters.P;

                    // note(lumip): The protocol as proposed by Naor and Pinkas includes a random value
                    //  to be incorporated in the random oracle query to ensure that the same query does
                    //  not occur several times. This is partly because they envision several receivers
                    //  over which the same Cs are used. Since we are having separate sets of Cs for each
                    //  sender-receiver pair, the requirement of unique queries is satisfied just using
                    //  the index j of the OT invocation and we can save a bit of bandwidth.

                    //  Think about whether we want to use a static set of Cs for each sender for all
                    //  connections to reduce the required amount of computation per OT. Would require to
                    //  maintain state in this class and negate the points made in the note above.
                    maskedOptions[j][i] = MaskOption(options[j][i], e, j, i);
                });
            });

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Computing masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            await WriteOptions(channel, maskedOptions, numberOfInvocations, numberOfMessageBytes);

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Sending masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
#endif
        }

        protected override async Task<Message[]> InternalReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif

            Quadruple<BigInteger> listOfCs = new Quadruple<BigInteger>(await ReadGroupElements(channel, 4));

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Reading c took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            BigInteger[] listOfBetas = new BigInteger[numberOfInvocations];
            BigInteger[] listOfDs = new BigInteger[numberOfInvocations];

            Parallel.For(0, numberOfInvocations, j =>
            {
                listOfDs[j] = GenerateGroupElement(out listOfBetas[j]);
                if (selectionIndices[j] > 0)
                    listOfDs[j] = listOfCs[selectionIndices[j]] * Invert(listOfDs[j]) % _parameters.P;
            });

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Generating d took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Task writeDsTask = WriteGroupElements(channel, listOfDs);
            Task<Quadruple<Message>[]> readMaskedOptionsTask = ReadOptions(channel, numberOfInvocations, numberOfMessageBytes);

            BigInteger[] listOfEs = new BigInteger[numberOfInvocations];
            Parallel.For(0, numberOfInvocations, j =>
            {
                listOfEs[j] = BigInteger.ModPow(listOfCs[0], listOfBetas[j], _parameters.P);
            });

            await Task.WhenAll(writeDsTask, readMaskedOptionsTask);
            Quadruple<Message>[] maskedOptions = readMaskedOptionsTask.Result;

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Computing e, sending d and reading masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Message[] selectedOptions = new Message[numberOfInvocations];
            Parallel.For(0, numberOfInvocations, j =>
            {
                int i = selectionIndices[j];
                BigInteger e = listOfEs[j];
                selectedOptions[j] = MaskOption(maskedOptions[j][i], e, j, i);
            });

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Unmasking result took {0} ms.", stopwatch.ElapsedMilliseconds);
#endif
            
            return selectedOptions;
        }

        private BigInteger GenerateGroupElement(out BigInteger exponent)
        {
            // note(lumip): Do not give in to the temptation of replacing the exponent > _parameters.Q part with a
            //  modulo operation, as that would cause the exponent to be no longer uniformly sampled (which could
            //  have an impact on security).
            do
            {
                exponent = RandomNumberGenerator.GetBigInteger(_parameters.ExponentSize);
            }
            while (exponent.IsZero || exponent > _parameters.Q);

            return BigInteger.ModPow(_parameters.G, exponent, _parameters.P);
        }

        private BigInteger Invert(BigInteger groupElement)
        {
            return BigInteger.ModPow(groupElement, _parameters.Q - 1, _parameters.P);
        }

        private Task WriteGroupElements(IMessageChannel channel, IReadOnlyList<BigInteger> groupElements)
        {
            Message message = new Message(groupElements.Count * _parameters.GroupElementSize);
            foreach (BigInteger groupElement in groupElements)
                message = message.Write(_parameters.GroupElementSize, groupElement);

            return channel.WriteMessageAsync(message);
        }

        private async Task<BigInteger[]> ReadGroupElements(IMessageChannel channel, int numberOfGroupElements)
        {
            Message message = await channel.ReadMessageAsync();

            BigInteger[] groupElements = new BigInteger[numberOfGroupElements];
            for (int i = 0; i < numberOfGroupElements; ++i)
                message = message.ReadBigInteger(_parameters.GroupElementSize, out groupElements[i]);

            return groupElements;
        }

        private Task WriteOptions(IMessageChannel channel, Quadruple<Message>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            Message message = new Message(4 * numberOfInvocations * numberOfMessageBytes);
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                for (int i = 0; i < 4; ++i)
                    message = message.Write(options[j][i]);
            }

            return channel.WriteMessageAsync(message);
        }

        private async Task<Quadruple<Message>[]> ReadOptions(IMessageChannel channel, int numberOfInvocations, int numberOfMessageBytes)
        {
            Message packedOptions = await channel.ReadMessageAsync();

            Quadruple<Message>[] options = new Quadruple<Message>[numberOfInvocations];
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                options[j] = new Quadruple<Message>();
                for (int i = 0; i < 4; ++i)
                {
                    packedOptions = packedOptions.ReadMessage(numberOfMessageBytes, out Message option);
                    options[j][i] = option;
                }
            }

            return options;
        }

        /// <summary>
        /// Masks an option (i.e., a sender input message).
        /// </summary>
        /// <remarks>
        /// The option is XOR-masked with the output of a random oracle queried with the
        /// concatenation of the binary representations of the given groupElement, invocationIndex and optionIndex.
        /// </remarks>
        /// <param name="option">The sender input/option to be masked.</param>
        /// <param name="groupElement">The group element that acts as "key" in the query to the random oracle.</param>
        /// <param name="invocationIndex">The index of the OT invocation this option belongs to.</param>
        /// <param name="optionIndex">The index of the option.</param>
        /// <returns>The masked option.</returns>
        private Message MaskOption(Message option, BigInteger groupElement, int invocationIndex, int optionIndex)
        {
            Message query = new Message(_parameters.GroupElementSize + 2 * sizeof(int))
                .Write(_parameters.GroupElementSize, groupElement)
                .Write(invocationIndex)
                .Write(optionIndex);
            
            return new Message(_randomOracle.Mask(option.ToBuffer(), query.ToBuffer()));
        }
    }
}
