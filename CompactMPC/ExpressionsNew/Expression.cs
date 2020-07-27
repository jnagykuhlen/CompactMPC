using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;

namespace CompactMPC.ExpressionsNew
{
    public abstract class Expression<T>
    {
        public IReadOnlyList<Wire> Wires { get; }

        protected Expression(IReadOnlyList<Wire> wires)
        {
            Wires = wires;
        }

        public T Evaluate(params IInputBinding[] inputBindings)
        {
            IReadOnlyList<ForwardGate> inputGates = inputBindings
                .SelectMany(inputBinding => inputBinding.Wires)
                .Select(wire => wire.Gate)
                .ToList();

            IReadOnlyList<ForwardGate> outputGates = Wires
                .Where(wire => !wire.IsConstant)
                .Select(wire => wire.Gate)
                .ToList();

            IReadOnlyList<Bit> inputBits = inputBindings
                .SelectMany(inputBinding => inputBinding.Bits)
                .ToList();

            ForwardCircuit circuit = new ForwardCircuit(inputGates, outputGates);
            IReadOnlyList<Bit> nonConstantOutputBits = circuit.Evaluate(LocalCircuitEvaluator.Instance, inputBits);

            int nextNonConstantOutputBitIndex = 0;
            Bit NextNonConstantOutputBit() => nonConstantOutputBits[nextNonConstantOutputBitIndex++];

            IReadOnlyList<Bit> outputBits = Wires
                .Select(wire => GetConstantValue(wire) ?? NextNonConstantOutputBit())
                .ToList();

            return Converter.FromBits(outputBits);
        }

        private static Bit? GetConstantValue(Wire wire)
        {
            if (wire == Wire.Zero)
                return Bit.Zero;

            if (wire == Wire.One)
                return Bit.One;

            return null;
        }

        public abstract IBitConverter<T> Converter { get; }
    }
}
