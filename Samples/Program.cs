using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using CompactMPC;
using CompactMPC.Networking;
using CompactMPC.Protocol;
using CompactMPC.ObliviousTransfer;

namespace CompactMPCDemo
{
    public class Program
    {
        private const int NumberOfParties = 3;
        private const int StartPort = 12348;
        
        public static void Main(string[] args)
        {
            BitArray[] inputs = new BitArray[]
            {
                BitArrayHelper.FromBinaryString("0111010011"),
                BitArrayHelper.FromBinaryString("1101100010"),
                BitArrayHelper.FromBinaryString("0111110011")
            };

            if (args.Length == 0)
            {
                Console.WriteLine("Starting coordinator...");

                string executablePath = Assembly.GetExecutingAssembly().Location;
                for (int i = 1; i < NumberOfParties; ++i)
                    Process.Start(executablePath, i.ToString());

                RunCompactMPCParty(0, inputs[0]);
            }
            else if (args.Length == 1)
            {
                Console.WriteLine("Starting client...");

                int localPartyId = Int32.Parse(args[0]);
                RunCompactMPCParty(localPartyId, inputs[localPartyId]);
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

        private static void RunCompactMPCParty(int localPartyId, BitArray localInput)
        {
            using (MultiPartyNetworkSession session = new MultiPartyNetworkSession(StartPort, NumberOfParties, localPartyId))
            {
                using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
                {
                    IBatchObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                        SecurityParameters.CreateDefault768Bit(),
                        cryptoContext
                    );

                    GMWSecureComputation computation = new GMWSecureComputation(session, obliviousTransfer, cryptoContext);
                    
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    
                    object[] outputPrimitives = (new SetIntersectionSecureProgram()).Evaluate(computation, new[] { localInput });
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
    }
}
