using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;
using CompactMPC.Protocol;

namespace CompactMPC.SampleCircuits
{
    public class SetIntersectionCircuitRecorder : ICircuitRecorder
    {
        private int _numberOfParties;
        private int _numberOfElements;
        private int _numberOfCounterBits;

        public SetIntersectionCircuitRecorder(int numberOfParties, int numberOfElements)
        {
            _numberOfParties = numberOfParties;
            _numberOfElements = numberOfElements;
            _numberOfCounterBits = CounterHelper.RequiredNumberOfBits(_numberOfElements);
        }

        public void Record(CircuitBuilder builder)
        {
            SecureWord[] inputs = new SecureWord[_numberOfParties];
            for (int i = 0; i < _numberOfParties; ++i)
                inputs[i] = Input(builder, _numberOfElements);

            SecureSetIntersection setIntersection = new SecureSetIntersection(inputs, _numberOfCounterBits);

            Output(builder, setIntersection.Intersection);
            Output(builder, setIntersection.Counter);
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

        public InputPartyMapping InputMapping
        {
            get
            {
                InputPartyMapping mapping = new InputPartyMapping(_numberOfParties * _numberOfElements);
                for (int i = 0; i < _numberOfParties; ++i)
                    mapping.AssignRange(i * _numberOfElements, _numberOfElements, i);

                return mapping;
            }
        }

        public OutputPartyMapping OutputMapping
        {
            get
            {
                OutputPartyMapping mapping = new OutputPartyMapping(_numberOfElements + _numberOfCounterBits);
                mapping.AssignRange(0, _numberOfElements + _numberOfCounterBits, IdSet.All);

                return mapping;
            }
        }
    }
}
