using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class NotGate : Gate
    {
        private Wire _input;

        public NotGate(Wire input)
        {
            _input = input;
        }

        public override T Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluation<T> evaluationState)
        {
            return evaluator.EvaluateNotGate(evaluationState.GetWireValue(_input));
        }
    }
}
