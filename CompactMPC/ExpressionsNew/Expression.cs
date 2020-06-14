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
            IReadOnlyList<Bit> outputBitsWithoutConstants = circuit.Evaluate(LocalCircuitEvaluator.Instance, inputBits);
            
            BitArray outputBits = new BitArray(Wires.Count);
            int nextOutputBitsWithoutConstantsIndex = 0;
            for (int i = 0; i < Wires.Count; ++i)
            {
                if (Wires[i] == Wire.Zero)
                {
                    outputBits[i] = Bit.Zero;
                }
                else if (Wires[i] == Wire.One)
                {
                    outputBits[i] = Bit.One;
                }
                else
                {
                    outputBits[i] = outputBitsWithoutConstants[nextOutputBitsWithoutConstantsIndex++];
                }
            }

            return Converter.FromBits(outputBits);
        }

        public abstract IBitConverter<T> Converter { get; }
    }
}
