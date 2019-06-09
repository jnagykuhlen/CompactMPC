using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class SecureComputationTest
    {
        private static readonly BitArray[] Inputs = new string[]
            {
                "010101",
                "110111",
                "010111",
                "110011",
                "110111"
            }.Select(input => BitArray.FromBinaryString(input)).ToArray();

        [TestMethod]
        public void TestTwoPartySetIntersection()
        {
            RunSecureComputationParties(12313, 2, "010101110");
        }

        [TestMethod]
        public void TestThreePartySetIntersection()
        {
            RunSecureComputationParties(12343, 3, "010101110");
        }

        [TestMethod]
        public void TestFourPartySetIntersection()
        {
            RunSecureComputationParties(12367, 4, "010001010");
        }

        [TestMethod]
        public void TestFivePartySetIntersection()
        {
            RunSecureComputationParties(12385, 5, "010001010");
        }

        private static void RunSecureComputationParties(int startPort, int numberOfParties, string expectedOutputString)
        {
            int numberOfElements = Inputs[0].Length;
            BitArray expectedOutput = BitArray.FromBinaryString(expectedOutputString);

            Task[] tasks = new Task[numberOfParties];
            for (int i = 0; i < numberOfParties; ++i)
            {
                int localPartyId = i;
                BitArray localInput = new BitArray(Inputs[localPartyId].ToArray());

                tasks[i] = Task.Factory.StartNew(
                    () => RunSecureComputationParty(startPort, numberOfParties, localPartyId, localInput, expectedOutput),
                    TaskCreationOptions.LongRunning
                );
            }

            Task.WaitAll(tasks);
        }

        private static void RunSecureComputationParty(int startPort, int numberOfParties, int localPartyId, BitArray localInput, BitArray expectedOutput)
        {
            using (TcpMultiPartyNetworkSession session = new TcpMultiPartyNetworkSession(startPort, numberOfParties, localPartyId))
            {
                using (CryptoContext cryptoContext = CryptoContext.CreateDefault())
                {
                    IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                        new SecurityParameters(47, 23, 4, 1, 1),
                        cryptoContext
                    );

                    IPairwiseMultiplicationScheme multiplicationScheme = new ObliviousTransferMultiplicationScheme(
                        obliviousTransfer,
                        cryptoContext
                    );

                    GMWSecureComputation computation = new GMWSecureComputation(session, multiplicationScheme, cryptoContext);
                    
                    SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(numberOfParties, localInput.Length);
                    CircuitBuilder circuitBuilder = new CircuitBuilder();
                    circuitRecorder.Record(circuitBuilder);

                    ForwardCircuit circuit = new ForwardCircuit(circuitBuilder.CreateCircuit());

                    Console.WriteLine("Input of party {0} is: {1}", localPartyId, localInput.ToBinaryString());

                    BitArray output = computation.EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput).Result;

                    Assert.AreEqual(
                        expectedOutput.Length,
                        output.Length,
                        "Incorrect output length {0} (should be {1}).",
                        output.Length,
                        expectedOutput.Length
                    );

                    Assert.IsTrue(
                        Enumerable.SequenceEqual(expectedOutput, output),
                        "Incorrect output {0} (should be {1}).",
                        output.ToBinaryString(),
                        expectedOutput.ToBinaryString()
                    );
                }
            }
        }
    }
}
