using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardOutputGate : ForwardGate
    {
        private int _outputIndex;

        public ForwardOutputGate(int outputIndex)
        {
            _outputIndex = outputIndex;
        }

        public override void Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            IReadOnlyList<T> inputValues = evaluationState.PullInputValues(this);
            evaluationState.SetOutput(_outputIndex, inputValues[0]);
        }

        public override int NumberOfInputs
        {
            get
            {
                return 1;
            }
        }
    }
}
