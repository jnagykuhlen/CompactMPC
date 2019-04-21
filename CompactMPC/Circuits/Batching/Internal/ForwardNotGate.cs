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

        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState, CircuitContext circuitContext)
        {
            T outputValue = evaluator.EvaluateNotGate(value, _context, circuitContext);
            SendOutputValue(outputValue, evaluator, evaluationState, circuitContext);
        }
    }
}
