using CompactMPC.Assertions;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Cryptography;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;
using CompactMPC.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class SecureComputationTest
    {
        private static readonly BitArray[] Inputs =
        {
            BitArray.FromBinaryString("010101"),
            BitArray.FromBinaryString("110111"),
            BitArray.FromBinaryString("010111"),
            BitArray.FromBinaryString("110011"),
            BitArray.FromBinaryString("110111")
        };

        [TestMethod]
        public void TestTwoPartySetIntersection()
        {
            RunSecureComputationParties(2, "010101110");
        }

        [TestMethod]
        public void TestThreePartySetIntersection()
        {
            RunSecureComputationParties(3, "010101110");
        }

        [TestMethod]
        public void TestFourPartySetIntersection()
        {
            RunSecureComputationParties(4, "010001010");
        }

        [TestMethod]
        public void TestFivePartySetIntersection()
        {
            RunSecureComputationParties(5, "010001010");
        }

        private static void RunSecureComputationParties(int numberOfParties, string expectedOutputString)
        {
            BitArray expectedOutput = BitArray.FromBinaryString(expectedOutputString);
            TestNetworkRunner.RunMultiPartyNetwork(
                numberOfParties,
                session => PerformSecureComputation(session, expectedOutput)
            );
        }

        private static void PerformSecureComputation(IMultiPartyNetworkSession session, BitArray expectedOutput)
        {
            BitArray localInput = Inputs[session.LocalParty.Id];
            
            using CryptoContext cryptoContext = CryptoContext.CreateDefault();

            IObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                new SecurityParameters(47, 23, 4, 1, 1),
                cryptoContext
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer,
                cryptoContext
            );

            SecretSharingSecureComputation computation = new SecretSharingSecureComputation(
                session,
                multiplicativeSharing,
                cryptoContext
            );

            SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(
                session.NumberOfParties,
                localInput.Length
            );

            CircuitBuilder circuitBuilder = new CircuitBuilder();
            circuitRecorder.Record(circuitBuilder);

            ForwardCircuit circuit = ForwardCircuit.FromCircuit(circuitBuilder.CreateCircuit());
            BitArray actualOutput = computation
                .EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput)
                .Result;

            EnumerableAssert.AreEqual(
                expectedOutput,
                actualOutput
            );
        }
    }
}
