using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Gates
{
    public class InputGate : Gate
    {
        public InputGate(GateContext context)
             : base(context) { }

        public override void Evaluate<TIn, TProcess, TOut>(
            ICircuitEvaluator<TIn, TProcess, TOut> evaluator,
            CircuitEvaluationState<TIn, TProcess, TOut> evaluationState,
            CircuitContext circuitContext)
        {
            TProcess value = evaluator.EvaluateInputGate(
                evaluationState.GetInput(Context.TypeUniqueId),
                Context,
                circuitContext
            );

            evaluationState.SetGateEvaluationValue(this, value);
        }

        public override IEnumerable<Gate> InputGates
        {
            get
            {
                return Enumerable.Empty<Gate>();
            }
        }
    }
}
