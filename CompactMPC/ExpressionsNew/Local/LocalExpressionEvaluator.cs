using System.Collections.Generic;
using System.Linq;
using CompactMPC.Circuits;
using CompactMPC.Circuits.Batching;

namespace CompactMPC.ExpressionsNew.Local
{
    public class LocalExpressionEvaluator
    {
        public T Evaluate<T>(IOutputExpression<T> expression, params ExpressionInputBinding[] inputBindings)
        {
            IReadOnlyList<ForwardGate> inputGates = inputBindings
                .SelectMany(inputBinding => inputBinding.Wires)
                .Select(wire => wire.Gate)
                .ToList();

            IReadOnlyList<ForwardGate> outputGates = expression.Wires
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

            IReadOnlyList<Bit> outputBits = expression.Wires
                .Select(wire => GetConstantValue(wire) ?? NextNonConstantOutputBit())
                .ToList();

            return expression.FromBits(outputBits);
        }

        private static Bit? GetConstantValue(Wire wire)
        {
            if (wire == Wire.Zero)
                return Bit.Zero;

            if (wire == Wire.One)
                return Bit.One;

            return null;
        }
    }
}