using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Gates
{
    public class NotGate : Gate
    {
        private Gate _inputGate;

        public NotGate(GateContext context, Gate inputGate)
             : base(context)
        {
            _inputGate = inputGate;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluationState<T> evaluationState,
            CircuitContext circuitContext)
        {
            T value = evaluator.EvaluateNotGate(
                evaluationState.GetGateEvaluationValue(_inputGate),
                Context,
                circuitContext
            );

            evaluationState.SetGateEvaluationValue(this, value);
        }

        public override IEnumerable<Gate> InputGates
        {
            get
            {
                yield return _inputGate;
            }
        }

        public Gate InputGate
        {
            get
            {
                return _inputGate;
            }
        }
    }
}
