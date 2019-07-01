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
    // note(lumip): the implementation does not seem to actually follow any construction given in
    // [Moni Naor and Benny Pinkas: Computationally Secure Oblivious Transfer. 2005.]
    // so what is the exact reference here?
    // looks like
    // [Moni Naor and Benny Pinkas: Efficient oblivious transfer protocols 2001.]
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
            // note(lumip): common argument verification code.. wrap into a common method in a base class?
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
            Quadruple<BigInteger> listOfExponents = new Quadruple<BigInteger>();
            
            Parallel.For(0, 4, i =>
            {
                BigInteger exponent;
                listOfCs[i] = GenerateGroupElement(out exponent);
                listOfExponents[i] = exponent;
            });

            BigInteger alpha = listOfExponents[0];
            // note(lumip): sender should probably verify that the all generated elements are different!
            //  otherwise the receiver could recover two or more of the sent values

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
                Parallel.For(0, 4, i =>
                {
                    BigInteger baseD = listOfDs[j];
                    if (i > 0)
                        baseD = listOfCs[i] * Invert(baseD);

                    BigInteger e = BigInteger.ModPow(baseD, alpha, _parameters.P);
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

        /// <summary>
        /// Returns a random element from the group as well as the corresponding exponent for the group generator.
        /// </summary>
        /// <param name="exponent">The exponent with which the returned group element can be obtained from the group generator.</param>
        /// <returns>A random group element.</returns>
        private BigInteger GenerateGroupElement(out BigInteger exponent)
        {
            do
            {
                exponent = _randomNumberGenerator.GetBigInteger(_parameters.ExponentSize);
            }
            while (exponent.IsZero || exponent > _parameters.Q);

            return BigInteger.ModPow(_parameters.G, exponent, _parameters.P);
        }

        /// <summary>
        /// Multiplicatively inverts a group element.
        /// </summary>
        /// <param name="groupElement">The group element to be inverted.</param>
        /// <returns>The multiplicative inverse of the argument in the group.</returns>
        private BigInteger Invert(BigInteger groupElement)
        {
            return BigInteger.ModPow(groupElement, _parameters.Q - 1, _parameters.P);
        }

        /// <summary>
        /// Asynchronously writes a list of group elements (BigInteger) to a message channel.
        /// </summary>
        /// <param name="channel">The network message channel.</param>
        /// <param name="groupElements">The list of group elements to write/send.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Asynchronously reads a specified number of group elements from a message channel.
        /// </summary>
        /// <param name="channel">The network message channel.</param>
        /// <param name="numberOfGroupElements">Number of group elements to read/receive.</param>
        /// <returns></returns>
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
        /// <param name="groupElement">The group element that contributes receiver choice to the query.</param>
        /// <param name="invocationIndex">The index of the OT invocation this options belongs to.</param>
        /// <param name="optionIndex">The index of the option.</param>
        /// <returns></returns>
        private byte[] MaskOption(byte[] option, BigInteger groupElement,  int invocationIndex, int optionIndex)
        {
            byte[] query = BufferBuilder.From(groupElement.ToByteArray()).With(invocationIndex).With(optionIndex).Create();
            return _randomOracle.Mask(option, query);
        }
    }
}
