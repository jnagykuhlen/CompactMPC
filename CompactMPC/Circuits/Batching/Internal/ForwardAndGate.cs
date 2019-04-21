using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardAndGate : ForwardGate
    {
        private GateContext _context;

        public ForwardAndGate(GateContext context)
        {
            _context = context;
        }

        public override void Evaluate<T>(IBatchedCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            IReadOnlyList<T> inputValues = evaluationState.PullInputValues(this);
            evaluationState.DelayAndGateEvaluation(new BatchElement<T>(new GateEvaluationInput<T>(_context, inputValues[0], inputValues[1]), Successors));
        }
        
        public override int NumberOfInputs
        {
            get
            {
                return 2;
            }
        }
    }
}
