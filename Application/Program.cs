using System;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Reflection;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;

namespace CompactMPC.Application
{
    public static class Program
    {
        private const int NumberOfParties = 3;
        private const int NumberOfElements = 10;
        private const int StartPort = 12348;

        public static void Main(string[] args)
        {
            BitArray[] inputs = {
                BitArray.FromBinaryString("0111010011"),
                BitArray.FromBinaryString("1101100010"),
                BitArray.FromBinaryString("0111110011")
            };
            
            if (args.Length == 0)
            {
                Console.WriteLine("Starting coordinator...");

                string executablePath = Assembly.GetExecutingAssembly().Location;
                for (int i = 1; i < NumberOfParties; ++i)
                    Process.Start(executablePath, i.ToString());

                RunSecureComputationParty(0, inputs[0]);
            }
            else if (args.Length == 1)
            {
                Console.WriteLine("Starting client...");

                int localPartyId = int.Parse(args[0]);
                RunSecureComputationParty(localPartyId, inputs[localPartyId]);
            }
            else
            {
                Console.WriteLine("Invalid number of arguments.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey(true);
        }

        private static void RunSecureComputationParty(int localPartyId, BitArray localInput)
        {
            using IMultiPartyNetworkSession session = CreateLocalSession(localPartyId, StartPort, NumberOfParties);
            using CryptoContext cryptoContext = CryptoContext.CreateDefault();
            
            IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                SecurityParameters.CreateDefault768Bit(),
                cryptoContext
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer,
                cryptoContext
            );

            GMWSecureComputation computation = new GMWSecureComputation(session, multiplicativeSharing, cryptoContext);

            Stopwatch stopwatch = Stopwatch.StartNew();

            SetIntersectionSecureProgram secureProgram = new SetIntersectionSecureProgram(NumberOfParties, NumberOfElements);
            object[] outputPrimitives = secureProgram.EvaluateAsync(computation, new object[] { localInput }).Result;
            BitArray intersection = (BitArray)outputPrimitives[0];
            BigInteger count = (BigInteger)outputPrimitives[1];

            stopwatch.Stop();

            Console.WriteLine();
            Console.WriteLine($"Completed protocol as {session.LocalParty.Name} in {stopwatch.ElapsedMilliseconds} ms.");
            Console.WriteLine($"  Local input: {localInput.ToBinaryString()}");
            Console.WriteLine($"  Computed intersection: {intersection.ToBinaryString()}");
            Console.WriteLine($"  Computed number of matches: {count}");
        }

        private static IMultiPartyNetworkSession CreateLocalSession(int localPartyId, int startPort, int numberOfParties)
        {
            return TcpMultiPartyNetworkSession.EstablishAsync(new Party(localPartyId), IPAddress.Loopback, startPort, numberOfParties).Result;
        }
    }
}
