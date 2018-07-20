using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CompactMPC.Circuits;
using CompactMPC.Expressions;
using CompactMPC.Protocol;

namespace CompactMPC.UnitTests
{
    public class SetIntersectionCircuitRecorder : ICircuitRecorder
    {
        private int _numberOfParties;
        private int _numberOfElements;

        public SetIntersectionCircuitRecorder(int numberOfParties, int numberOfElements)
        {
            _numberOfParties = numberOfParties;
            _numberOfElements = numberOfElements;
        }

        public void Record(CircuitBuilder builder)
        {
            SecureWord[] inputs = new SecureWord[_numberOfParties];
            for (int i = 0; i < _numberOfParties; ++i)
                inputs[i] = new SecureWord(builder, InputBlock(builder, _numberOfElements));

            SecureWord output = inputs.AggregateDepthEfficient((x, y) => x & y);
            OutputBlock(builder, output.Wires);
        }

        private static Wire[] InputBlock(CircuitBuilder builder, int length)
        {
            Wire[] result = new Wire[length];
            for (int i = 0; i < length; ++i)
                result[i] = builder.Input();
            return result;
        }

        private static void OutputBlock(CircuitBuilder builder, IEnumerable<Wire> block)
        {
            foreach (Wire bit in block)
                builder.Output(bit);
        }

        public InputPartyMapping InputMapping
        {
            get
            {
                InputPartyMapping inputMapping = new InputPartyMapping(_numberOfParties * _numberOfElements);
                for (int partyId = 0; partyId < _numberOfParties; ++partyId)
                    inputMapping.AssignRange(partyId * _numberOfElements, _numberOfElements, partyId);

                return inputMapping;
            }
        }

        public OutputPartyMapping OutputMapping
        {
            get
            {
                OutputPartyMapping outputMapping = new OutputPartyMapping(_numberOfElements);
                outputMapping.AssignRange(0, _numberOfElements, IdSet.All);
                return outputMapping;
            }
        }
    }
}
