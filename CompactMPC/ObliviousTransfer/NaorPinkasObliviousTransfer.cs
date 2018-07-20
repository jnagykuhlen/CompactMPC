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

namespace CompactMPC.ObliviousTransfer
{
    public class NaorPinkasObliviousTransfer : IBatchObliviousTransfer
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

        public Task SendAsync(Stream stream, Quadruple<byte[]>[] options, int numberOfMessageBytes, int numberOfInvocations)
        {
            if (options.Length != numberOfInvocations)
                throw new ArgumentException("Provided options must match the specified number of invocations.", nameof(options));
            
            for (int i = 0; i < options.Length; ++i)
            {
                foreach (byte[] message in options[i])
                {
                    if (message.Length != numberOfMessageBytes)
                        throw new ArgumentException("Length of provided options does not match the specified message length.", nameof(options));
                }
            }
            
            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true);

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

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Generating group elements took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            for (int i = 0; i < 4; ++i)
            {
                byte[] data = listOfCs[i].ToByteArray();
                writer.Write(data.Length);
                writer.Write(data);
            }

            writer.Flush();

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Sending values for c took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif
            
            BigInteger[] listOfDs = new BigInteger[numberOfInvocations];
            for (int j = 0; j < numberOfInvocations; ++j)
            {
                int size = reader.ReadInt32();
                byte[] data = reader.ReadBytes(size);

                listOfDs[j] = new BigInteger(data);
            }

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
                    
                    maskedOptions[j][i] = _randomOracle.Mask(
                        options[j][i],
                        CombineBuffers(e.ToByteArray(), BitConverter.GetBytes(j), BitConverter.GetBytes(i))
                    );
                });
            });

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Computing masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif

            for (int j = 0; j < numberOfInvocations; ++j)
            {
                for (int i = 0; i < 4; ++i)
                    writer.Write(maskedOptions[j][i]);
            }

            writer.Flush();

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Sender] Sending masked options took {0} ms.", stopwatch.ElapsedMilliseconds);
#endif

            return Task.CompletedTask;
        }

        public Task<byte[][]> ReceiveAsync(Stream stream, int[] selectionIndices, int numberOfMessageBytes, int numberOfInvocations)
        {
            if (selectionIndices.Length != numberOfInvocations)
                throw new ArgumentException("Provided selection indices must match the specified number of invocations.", nameof(selectionIndices));

            for (int i = 0; i < selectionIndices.Length; ++i)
            {
                if (selectionIndices[i] < 0 || selectionIndices[i] >= 4)
                    throw new ArgumentOutOfRangeException(nameof(selectionIndices), "Invalid selection index for 1-out-of-4 oblivious transfer.");
            }

            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
            BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true);

#if DEBUG
            Stopwatch stopwatch = Stopwatch.StartNew();
#endif

            Quadruple<BigInteger> listOfCs = new Quadruple<BigInteger>();

            for (int i = 0; i < 4; ++i)
            {
                int size = reader.ReadInt32();
                byte[] data = reader.ReadBytes(size);

                listOfCs[i] = new BigInteger(data);
            }

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

            for (int j = 0; j < numberOfInvocations; ++j)
            {
                byte[] data = listOfDs[j].ToByteArray();
                writer.Write(data.Length);
                writer.Write(data);
            }

            writer.Flush();

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Generating and writing d took {0} ms.", stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
#endif
            
            Quadruple<byte[]>[] maskedOptions = new Quadruple<byte[]>[numberOfInvocations];

            for (int j = 0; j < numberOfInvocations; ++j)
            {
                maskedOptions[j] = new Quadruple<byte[]>();
                for (int i = 0; i < 4; ++i)
                    maskedOptions[j][i] = reader.ReadBytes(numberOfMessageBytes);
            }

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
                
                selectedOptions[j] = _randomOracle.Mask(
                    maskedOptions[j][i],
                    CombineBuffers(e.ToByteArray(), BitConverter.GetBytes(j), BitConverter.GetBytes(i))
                );
            });

#if DEBUG
            stopwatch.Stop();
            Console.WriteLine("[Receiver] Unmasking result took {0} ms.", stopwatch.ElapsedMilliseconds);
#endif
            
            return Task.FromResult(selectedOptions);
        }

        private BigInteger GenerateGroupElement(out BigInteger exponent)
        {
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

        private byte[] CombineBuffers(params byte[][] buffers)
        {
            byte[] result = new byte[buffers.Sum(buffer => buffer.Length)];

            int offset = 0;
            for (int i = 0; i < buffers.Length; ++i)
            {
                byte[] buffer = buffers[i];
                Buffer.BlockCopy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }

            return result;
        }
    }
}
