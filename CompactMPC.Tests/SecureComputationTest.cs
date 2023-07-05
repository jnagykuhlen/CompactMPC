using System.Threading.Tasks;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;
using CompactMPC.Networking;
using CompactMPC.ObliviousTransfer;
using CompactMPC.Protocol;
using CompactMPC.SampleCircuits;
using CompactMPC.Util;
using FluentAssertions;
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
        public Task TestTwoPartySetIntersection()
        {
            return RunSecureComputationParties(2, "010101110");
        }

        [TestMethod]
        public Task TestThreePartySetIntersection()
        {
            return RunSecureComputationParties(3, "010101110");
        }

        [TestMethod]
        public Task TestFourPartySetIntersection()
        {
            return RunSecureComputationParties(4, "010001010");
        }

        [TestMethod]
        public Task TestFivePartySetIntersection()
        {
            return RunSecureComputationParties(5, "010001010");
        }

        private static Task RunSecureComputationParties(int numberOfParties, string expectedOutputString)
        {
            BitArray expectedOutput = BitArray.FromBinaryString(expectedOutputString);
            return TestNetworkRunner.RunMultiPartyNetwork(
                numberOfParties,
                session => PerformSecureComputation(session, expectedOutput)
            );
        }

        private static void PerformSecureComputation(IMultiPartyNetworkSession session, BitArray expectedOutput)
        {
            BitArray localInput = Inputs[session.LocalParty.Id];

            IBitObliviousTransfer obliviousTransfer = new NaorPinkasObliviousTransfer(
                new SecurityParameters(47, 23, 4, 1, 1)
            );

            IMultiplicativeSharing multiplicativeSharing = new ObliviousTransferMultiplicativeSharing(
                obliviousTransfer
            );

            SecretSharingSecureComputation computation = new SecretSharingSecureComputation(
                session,
                multiplicativeSharing
            );

            SetIntersectionCircuitRecorder circuitRecorder = new SetIntersectionCircuitRecorder(
                session.NumberOfParties,
                localInput.Length
            );

            CircuitBuilder circuitBuilder = new CircuitBuilder();
            circuitRecorder.Record(circuitBuilder);

            ForwardCircuit circuit = new ForwardCircuit(circuitBuilder.CreateCircuit());
            BitArray actualOutput = computation
                .EvaluateAsync(circuit, circuitRecorder.InputMapping, circuitRecorder.OutputMapping, localInput)
                .Result;

            actualOutput.Should().Equal(expectedOutput);
        }
    }
}