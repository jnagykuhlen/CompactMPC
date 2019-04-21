using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardXorGate : BinaryForwardGate
    {
        private GateContext _context;

        public ForwardXorGate(GateContext context)
        {
            _context = context;
        }

        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            T outputValue = evaluator.EvaluateXorGate(leftValue, rightValue, _context, circuitContext);
            SendOutputValue(outputValue, evaluator, evaluationState, circuitContext);
        }
    }
}
