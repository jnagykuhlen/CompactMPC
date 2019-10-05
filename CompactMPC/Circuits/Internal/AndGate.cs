using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class AndGate : Gate
    {
        private Wire _leftInput;
        private Wire _rightInput;

        public AndGate(Wire leftInput, Wire rightInput)
        {
            _leftInput = leftInput;
            _rightInput = rightInput;
        }
        
        public override T Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluation<T> evaluationState)
        {
            return evaluator.EvaluateAndGate(
                evaluationState.GetWireValue(_leftInput),
                evaluationState.GetWireValue(_rightInput)
            );
        }
    }
}
