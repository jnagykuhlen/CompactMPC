using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardAndGate : BinaryForwardGate
    {
        private GateContext _context;

        public ForwardAndGate(GateContext context)
        {
            _context = context;
        }

        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            GateEvaluationInput<T> evaluationInput = new GateEvaluationInput<T>(_context, leftValue, rightValue);
            evaluationState.DelayAndGateEvaluation(new GateEvaluation<T>(this, evaluationInput));
        }
    }
}
