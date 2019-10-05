using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class XorGate : Gate
    {
        private Wire _leftInput;
        private Wire _rightInput;

        public XorGate(Wire leftInput, Wire rightInput)
        {
            _leftInput = leftInput;
            _rightInput = rightInput;
        }

        public override T Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluation<T> evaluationState)
        {
            return evaluator.EvaluateXorGate(
                evaluationState.GetWireValue(_leftInput),
                evaluationState.GetWireValue(_rightInput)
            );
        }
    }
}
