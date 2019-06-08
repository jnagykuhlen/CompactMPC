using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;

namespace CompactMPC.Samples
{
    public class SetIntersectionCircuitRecorder : ICircuitRecorder
    {
        private const int NumberOfElements = 10;
        private const int NumberOfCounterBits = 4;

        public void Record(CircuitBuilder builder)
        {
            SecureWord inputA = Input(builder, NumberOfElements);
            SecureWord inputB = Input(builder, NumberOfElements);
            SecureWord inputC = Input(builder, NumberOfElements);
            
            SecureWord intersection = SecureWord.And(inputA, inputB, inputC);

            SecureInteger counter = SecureInteger.Zero(builder);

            for (int i = 0; i < NumberOfElements; ++i)
                counter = counter + SecureInteger.FromBoolean(inputA.IsBitSet(i));

            counter = counter.OfFixedLength(NumberOfCounterBits);
            
            Output(builder, intersection);
            Output(builder, counter);
        }

        private SecureWord Input(CircuitBuilder builder, int numberOfBits)
        {
            Wire[] wires = new Wire[numberOfBits];
            for (int i = 0; i < numberOfBits; ++i)
                wires[i] = builder.Input();

            return new SecureWord(builder, wires);
        }

        private void Output(CircuitBuilder builder, SecureWord word)
        {
            foreach (Wire wire in word.Wires)
                builder.Output(wire);
        }
    }
}
