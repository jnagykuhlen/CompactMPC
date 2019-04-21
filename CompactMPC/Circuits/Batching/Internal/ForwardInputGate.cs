using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardInputGate : ForwardGate
    {
        private int _inputIndex;

        public ForwardInputGate(int inputIndex)
        {
            _inputIndex = inputIndex;
        }

        public override void Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            T inputValue = evaluationState.GetInput(_inputIndex);
            foreach (ForwardGate successor in Successors)
            {
                evaluationState.PushInputValue(successor, inputValue);
                if (evaluationState.GetNumberOfInputValues(successor) >= successor.NumberOfInputs)
                    successor.Evaluate(evaluator, evaluationState, circuitContext);
            }
        }

        public override int NumberOfInputs
        {
            get
            {
                return 0;
            }
        }
    }
}
