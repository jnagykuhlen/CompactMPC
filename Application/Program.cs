using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Statistics;
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

            CircuitBuilder builder = new CircuitBuilder();
            (new SetIntersectionCircuitRecorder(NumberOfParties, NumberOfElements)).Record(builder);

            Circuit circuit = builder.CreateCircuit();

            CircuitStatistics statistics = CircuitStatistics.FromCircuit(circuit);

            Console.WriteLine("--- Circuit Statistics ---");
            Console.WriteLine("Number of inputs: {0}", statistics.NumberOfInputs);
            Console.WriteLine("Number of outputs: {0}", statistics.NumberOfOutputs);
            Console.WriteLine("Number of ANDs: {0}", circuit.Context.NumberOfAndGates);
            Console.WriteLine("Number of XORs: {0}", circuit.Context.NumberOfXorGates);
            Console.WriteLine("Number of NOTs: {0}", circuit.Context.NumberOfNotGates);
            Console.WriteLine("  Total linear: {0}", circuit.Context.NumberOfXorGates + circuit.Context.NumberOfNotGates);
            Console.WriteLine("Multiplicative depth: {0}", statistics.Layers.Count);

            int totalNonlinearGates = 0;
            int totalLinearGates = 0;

            for (int i = 0; i < statistics.Layers.Count; ++i)
            {
                totalNonlinearGates += statistics.Layers[i].NumberOfNonlinearGates;
                totalLinearGates += statistics.Layers[i].NumberOfLinearGates;
                Console.WriteLine("  Layer {0}: {1} nonlinear / {2} linear", i, statistics.Layers[i].NumberOfNonlinearGates, statistics.Layers[i].NumberOfLinearGates);
            }

            Console.WriteLine("Number of nonlinear gates in layers: {0}", totalNonlinearGates);
            Console.WriteLine("Number of linear gates in layers: {0}", totalLinearGates);
            
            Bit[] result;
            
            result = circuit.Evaluate(new LocalCircuitEvaluator(), inputs.SelectMany(input => input).ToArray());
            Console.WriteLine("Result (normal): {0}", new BitArray(result).ToBinaryString());

            result = new Circuits.Batching.ForwardCircuit(circuit).Evaluate(new LocalCircuitEvaluator(), inputs.SelectMany(input => input).ToArray());
            Console.WriteLine("Result (forward): {0}", new BitArray(result).ToBinaryString());
            
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
            using (TcpMultiPartyNetworkSession session = new TcpMultiPartyNetworkSession(StartPort, NumberOfParties, localPartyId))
            {
                using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
                {
                    IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                        SecurityParameters.CreateDefault768Bit(),
                        cryptoContext
                    );

                    IPairwiseMultiplicationScheme multiplicationScheme = new ObliviousTransferMultiplicationScheme(
                        obliviousTransfer,
                        cryptoContext
                    );

                    GMWSecureComputation computation = new GMWSecureComputation(session, multiplicationScheme, cryptoContext);

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
    }
}
