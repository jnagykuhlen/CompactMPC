using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardCircuitBuildingEvaluator : ICircuitEvaluator<ForwardGate>
    {
        private int _numberOfAndGates;
        private int _numberOfXorGates;
        private int _numberOfNotGates;

        public static ForwardGate[] Build(Circuit circuit)
        {
            ForwardGate[] inputGates = new ForwardGate[circuit.InputWires.Count];
            for (int i = 0; i < inputGates.Length; ++i)
                inputGates[i] = new ForwardInputGate(i);

            ForwardGate[] outputGates = circuit.Evaluate(new ForwardCircuitBuildingEvaluator(), inputGates);
            for (int i = 0; i < outputGates.Length; ++i)
                outputGates[i].AddSuccessor(new ForwardOutputGate(i));

            return inputGates;
        }

        public ForwardGate EvaluateAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            ForwardGate gate = new ForwardAndGate();
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);

            _numberOfAndGates++;

            return gate;
        }

        public ForwardGate EvaluateXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            ForwardGate gate = new ForwardXorGate();
            leftInputGate.AddSuccessor(gate);
            rightInputGate.AddSuccessor(gate);

            _numberOfXorGates++;

            return gate;
        }

        public ForwardGate EvaluateNotGate(ForwardGate inputGate)
        {
            ForwardGate gate = new ForwardNotGate();
            inputGate.AddSuccessor(gate);

            _numberOfNotGates++;

            return gate;
        }

        public int NumberOfAndGates
        {
            get
            {
                return _numberOfAndGates;
            }
        }

        public int NumberOfXorGates
        {
            get
            {
                return _numberOfXorGates;
            }
        }

        public int NumberOfNotGates
        {
            get
            {
                return _numberOfNotGates; 
            }
        }
    }
}
