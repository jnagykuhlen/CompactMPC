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

        public override void Evaluate<TIn, TProcess, TOut>(
            ICircuitEvaluator<TIn, TProcess, TOut> evaluator,
            CircuitEvaluationState<TIn, TProcess, TOut> evaluationState,
            CircuitContext circuitContext)
        {
            TProcess value = evaluator.EvaluateNotGate(
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
