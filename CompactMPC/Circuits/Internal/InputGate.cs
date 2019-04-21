using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class InputGate : Gate
    {
        public InputGate(GateContext context)
             : base(context) { }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState,
            CircuitContext circuitContext)
        {
            evaluationState.SetGateEvaluationValue(this, evaluationState.GetInput(Context.TypeUniqueId));
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
