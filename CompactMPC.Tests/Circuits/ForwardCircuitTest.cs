﻿using System;
using CompactMPC.Circuits.Batching;
using CompactMPC.Circuits.Batching.Internal;
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

            Assert.AreEqual(3, circuit.Context.NumberOfAndGates);
            Assert.AreEqual(2, circuit.Context.NumberOfXorGates);
            Assert.AreEqual(1, circuit.Context.NumberOfNotGates);
            Assert.AreEqual(2, circuit.Context.NumberOfInputWires);
            Assert.AreEqual(3, circuit.Context.NumberOfOutputWires);
            Assert.AreEqual(11, circuit.Context.NumberOfGates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestDuplicatedInputGates()
        {
            ForwardGate firstInputGate = new ForwardInputGate();
            ForwardGate secondInputGate = new ForwardInputGate();
            ForwardGate outputGate = new ForwardXorGate(firstInputGate, secondInputGate);
            
            ForwardCircuit circuit = new ForwardCircuit(
                new[] { firstInputGate, secondInputGate, firstInputGate },
                new[] { outputGate }
            );
        }
    }
}
