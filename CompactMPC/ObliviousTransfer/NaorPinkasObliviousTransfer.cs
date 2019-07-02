using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Diagnostics;

using CompactMPC.Networking;
using CompactMPC.Buffers;

namespace CompactMPC.ObliviousTransfer
{
    /// <summary>
    /// 1-out-of-4 Oblivious Transfer Implementation following a protocol by Naor and Pinkas.
    /// </summary>
    /// 
    /// Reference: Moni Naor and Benny Pinkas: Efficient oblivious transfer protocols 2001. https://dl.acm.org/citation.cfm?id=365502
    public class NaorPinkasObliviousTransfer : GeneralizedObliviousTransfer
    {
        private SecurityParameters _parameters;
        private RandomOracle _randomOracle;
        private RandomNumberGenerator _randomNumberGenerator;
        
        public NaorPinkasObliviousTransfer(SecurityParameters parameters, CryptoContext cryptoContext)
        {
            _parameters = parameters;
            _randomOracle = new HashRandomOracle(cryptoContext.HashAlgorithm);
            _randomNumberGenerator = new ThreadsafeRandomNumberGenerator(cryptoContext.RandomNumberGenerator);
#if DEBUG
            Console.WriteLine("Security parameters:");
            Console.WriteLine("p = {0}", _parameters.P);
            Console.WriteLine("q = {0}", _parameters.Q);
            Console.WriteLine("g = {0}", _parameters.G);
            Console.WriteLine("group element size = {0} bytes", _parameters.GroupElementSize);
            Console.WriteLine("exponent size = {0} bytes", _parameters.ExponentSize);
#endif
        }

        public override async Task SendAsync(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));
            
            for (int j = 0; j < options.Length; ++j)
            {
                foreach (byte[] message in options[j])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("Length of provided options does not match the specified message length.", nameof(options));
                }
            }
            
#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif
            
            Quadruple<BigInteger> listOfCs = new Quadruple<BigInteger>();
            Quadruple<BigInteger> listOfExpCs = new Quadruple<BigInteger>();

            BigInteger alpha;
            listOfCs[0] = GenerateGroupElement(out alpha);
            listOfExpCs[0] = listOfCs[0];
            
            Parallel.For(1, 4, i =>
            {
                BigInteger exponent;
                listOfCs[i] = GenerateGroupElement(out exponent);
                listOfExpCs[i] = BigInteger.ModPow(listOfCs[i], alpha, _parameters.P);
            });


#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Generating group elements took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            await WriteGroupElements(channel, listOfCs);

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Sending values for c took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            BigInteger[] listOfDs = await ReadGroupElements(channel, numberOfInvocations);

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Reading d took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Quadruple<byte[]>[] maskedOptions = new Quadruple<byte[]>[numberOfInvocations];
            Parallel.For(0, numberOfInvocations, j =>
            {
                maskedOptions[j] = new Quadruple<byte[]>();
                BigInteger baseExpD = BigInteger.ModPow(listOfDs[j], alpha, _parameters.P);
                BigInteger invBaseExpD = Invert(baseExpD);

                Parallel.For(0, 4, i =>
                {
                    BigInteger expD;
                    if (i == 0)
                    {
                        expD = baseExpD;
                    }
                    else
                    {
                        expD = (listOfExpCs[i] * invBaseExpD) % _parameters.P;
                    }

                    // note(lumip): the protocol as proposed by Naor and Pinkas includes a random value
                    //  to be incorporated in the random oracle query to ensure that the same query does
                    //  not occur several times. This is partly because the envision several receivers
                    //  over which the same Cs are used. Since we are having seperate sets of Cs for each
                    //  sender-receiver pair, the requirement of unique queries is satisified just using
                    //  the index j of the OT invocation and we can save a bit of bandwidth.

                    // todo: think about whether we want to use a static set of Cs for each sender for all
                    //  connection to reduce the required amount of computation per OT. Would require to
                    //  maintain state in this class and negate the points made in the note above.
                    maskedOptions[j][i] = MaskOption(options[j][i], expD, j, i);
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

        public override async Task<byte[][]> ReceiveAsync(IMessageChannel channel, QuadrupleIndexArray selectionIndices, int numberOfInvocations, int numberOfMessageBytes)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif

            Quadruple<BigInteger> listOfCs = new Quadruple<BigInteger>(await ReadGroupElements(channel, 4));

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Reading values for c took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            BigInteger[] listOfBetas = new BigInteger[numberOfInvocations];
            BigInteger[] listOfDs = new BigInteger[numberOfInvocations];

            Parallel.For(0, numberOfInvocations, j =>
            {
                listOfDs[j] = GenerateGroupElement(out listOfBetas[j]);
                if (selectionIndices[j] > 0)
                    listOfDs[j] = (listOfCs[selectionIndices[j]] * Invert(listOfDs[j])) % _parameters.P;
            });

            await WriteGroupElements(channel, listOfDs);

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Generating and writing d took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            Quadruple<byte[]>[] maskedOptions = await ReadOptions(channel, numberOfInvocations, numberOfMessageBytes);

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Reading masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            byte[][] selectedOptions = new byte[numberOfInvocations][];
            Parallel.For(0, numberOfInvocations, j =>
            {
                int i = selectionIndices[j];
                BigInteger e = BigInteger.ModPow(listOfCs[0], listOfBetas[j], _parameters.P);
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
            // note(lumip): do not give in to the temptation of replacing the exponent > _parameters.Q part with a
            //  modulo operation, as that would case the exponent to be no longer uniformly sampled (which could
            //  have an impact on security)
            do
            {
                exponent = _randomNumberGenerator.GetBigInteger(_parameters.ExponentSize);
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
            MessageComposer message = new MessageComposer(2 * groupElements.Count);
            foreach (BigInteger groupElement in groupElements)
            {
                byte[] packedGroupElement = groupElement.ToByteArray();
                message.Write(packedGroupElement.Length);
                message.Write(packedGroupElement);
            }

            return channel.WriteMessageAsync(message.Compose());
        }

        private async Task<BigInteger[]> ReadGroupElements(IMessageChannel channel, int numberOfGroupElements)
        {
            MessageDecomposer message = new MessageDecomposer(await channel.ReadMessageAsync());

            BigInteger[] groupElements = new BigInteger[numberOfGroupElements];
            for (int i = 0; i < numberOfGroupElements; ++i)
            {
                int length = message.ReadInt();
                byte[] packedGroupElement = message.ReadBuffer(length);
                groupElements[i] = new BigInteger(packedGroupElement);
            }

            return groupElements;
        }

        private Task WriteOptions(IMessageChannel channel, Quadruple<byte[]>[] options, int numberOfInvocations, int numberOfMessageBytes)
        {
            MessageComposer message = new MessageComposer(4 * numberOfInvocations);
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                for (int i = 0; i < 4; ++i)
                    message.Write(options[j][i]);
            }

            return channel.WriteMessageAsync(message.Compose());
        }

        private async Task<Quadruple<byte[]>[]> ReadOptions(IMessageChannel channel, int numberOfInvocations, int numberOfMessageBytes)
        {
            MessageDecomposer message = new MessageDecomposer(await channel.ReadMessageAsync());

            Quadruple<byte[]>[] options = new Quadruple<byte[]>[numberOfInvocations];
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                options[j] = new Quadruple<byte[]>();
                for (int i = 0; i < 4; ++i)
                    options[j][i] = message.ReadBuffer(numberOfMessageBytes);
            }

            return options;
        }

        /// <summary>
        /// Masks an option (i.e., a sender input message).
        /// </summary>
        /// 
        /// The option is XOR-masked with the output of a random oracle queried with the
        /// concatentation of the binary representations of the given groupElement, invocationIndex and optionIndex.
        /// 
        /// <param name="option">The sender input/option to be masked.</param>
        /// <param name="groupElement">The group element that acts as "key" in the query to the random oracle.</param>
        /// <param name="invocationIndex">The index of the OT invocation this options belongs to.</param>
        /// <param name="optionIndex">The index of the option.</param>
        /// <returns>The masked option.</returns>
        private byte[] MaskOption(byte[] option, BigInteger groupElement,  int invocationIndex, int optionIndex)
        {
            byte[] query = BufferBuilder.From(groupElement.ToByteArray()).With(invocationIndex).With(optionIndex).Create();
            return _randomOracle.Mask(option, query);
        }
    }
}
