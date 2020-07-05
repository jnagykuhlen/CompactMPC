using System;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Circuits
{
    [TestClass]
    public class ForwardCircuitTest
    {
        [TestMethod]
        public void TestGateCount()
        {
            ForwardGate firstInputGate = new ForwardInputGate();
            ForwardGate secondInputGate = new ForwardInputGate();
            ForwardGate firstXorGate = new ForwardXorGate(firstInputGate, secondInputGate);
            ForwardGate firstAndGate = new ForwardAndGate(firstXorGate, secondInputGate);
            ForwardGate secondAndGate = new ForwardAndGate(firstXorGate, firstAndGate);
            ForwardGate thirdAndGate = new ForwardAndGate(firstInputGate, secondInputGate);
            ForwardGate secondXorGate = new ForwardXorGate(secondAndGate, thirdAndGate);
            ForwardNotGate firstNotGate = new ForwardNotGate(firstAndGate);
            
            ForwardCircuit circuit = new ForwardCircuit(
                new[] { firstInputGate, secondInputGate },
                new[] { firstAndGate, firstNotGate, secondXorGate }
            );

            circuit.Context.NumberOfAndGates.Should().Be(3);
            circuit.Context.NumberOfXorGates.Should().Be(2);
            circuit.Context.NumberOfNotGates.Should().Be(1);
            circuit.Context.NumberOfInputWires.Should().Be(2);
            circuit.Context.NumberOfOutputWires.Should().Be(3);
            circuit.Context.NumberOfGates.Should().Be(11);
        }

        [TestMethod]
        public void TestDuplicatedInputGates()
        {
            ForwardGate firstInputGate = new ForwardInputGate();
            ForwardGate secondInputGate = new ForwardInputGate();
            ForwardGate outputGate = new ForwardXorGate(firstInputGate, secondInputGate);
            
            Func<ForwardCircuit> createCircuit = () => new ForwardCircuit(
                new[] { firstInputGate, secondInputGate, firstInputGate },
                new[] { outputGate }
            );

            createCircuit.Should().Throw<ArgumentException>();
        }
    }
}
