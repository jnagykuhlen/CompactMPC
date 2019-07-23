using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Net;

using CompactMPC.Networking;
using CompactMPC.Protocol;
using CompactMPC.ObliviousTransfer;
using CompactMPC.SampleCircuits;

namespace CompactMPC.Application
{
    public class Program
    {
        private const int NumberOfParties = 3;
        private const int NumberOfElements = 10;
        private const int StartPort = 12348;

        public static void Main(string[] args)
        {
            BitArray[] inputs = new BitArray[]
            {
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

                int localPartyId = Int32.Parse(args[0]);
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
            using (IMultiPartyNetworkSession session = CreateLocalSession(localPartyId, StartPort, NumberOfParties))
            {
                using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
                {
                    IObliviousTransferProvider<IMultiChoicesBitObliviousTransferChannel> obliviousTransfer =
                        new MultiChoicesBitObliviousTransferProviderAdapter(
                            new StatelessMultiChoicesObliviousTransferProvider(
                                new NaorPinkasObliviousTransfer(
                                    SecurityParameters.CreateDefault768Bit(),
                                    cryptoContext
                                )
                        )
                    );

                    IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                        obliviousTransfer,
                        cryptoContext
                    );

                    GMWSecureComputation computation = new GMWSecureComputation(session, multiplicativeSharing, cryptoContext);

                    Stopwatch stopwatch = Stopwatch.StartNew();

                    SetIntersectionSecureProgram secureProgram = new SetIntersectionSecureProgram(NumberOfParties, NumberOfElements);
                    object[] outputPrimitives = secureProgram.EvaluateAsync(computation, new[] { localInput }).Result;
                    BitArray intersection = (BitArray)outputPrimitives[0];
                    BigInteger count = (BigInteger)outputPrimitives[1];

                    stopwatch.Stop();

                    Console.WriteLine();
                    Console.WriteLine("Completed protocol as {0} in {1} ms.", session.LocalParty.Name, stopwatch.ElapsedMilliseconds);
                    Console.WriteLine("  Local input: {0}", localInput.ToBinaryString());
                    Console.WriteLine("  Computed intersection: {0}", intersection.ToBinaryString());
                    Console.WriteLine("  Computed number of matches: {0}", count);
                }
            }
        }

        private static IMultiPartyNetworkSession CreateLocalSession(int localPartyId, int startPort, int numberOfParties)
        {
            return TcpMultiPartyNetworkSession.EstablishAsync(new Party(localPartyId), IPAddress.Loopback, StartPort, NumberOfParties).Result;
        }
    }
}
