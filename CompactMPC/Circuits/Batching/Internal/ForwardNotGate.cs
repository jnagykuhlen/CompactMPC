using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardNotGate : ForwardGate
    {
        private GateContext _context;

        public ForwardNotGate(GateContext context)
        {
            _context = context;
        }

        public override void Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            IReadOnlyList<T> inputValues = evaluationState.PullInputValues(this);
            T outputValue = evaluator.EvaluateNotGate(inputValues[0], _context, circuitContext);

            foreach (ForwardGate successor in Successors)
            {
                evaluationState.PushInputValue(successor, outputValue);
                if (evaluationState.GetNumberOfInputValues(successor) >= successor.NumberOfInputs)
                    successor.Evaluate(evaluator, evaluationState, circuitContext);
            }
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
