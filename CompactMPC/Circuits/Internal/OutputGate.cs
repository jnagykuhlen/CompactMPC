using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class OutputGate : Gate
    {
        private Gate _inputGate;

        public OutputGate(GateContext context, Gate inputGate)
             : base(context)
        {
            _inputGate = inputGate;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState,
            CircuitContext circuitContext)
        {
            evaluationState.SetOutput(Context.TypeUniqueId, evaluationState.GetGateEvaluationValue(_inputGate));
        }

        public override IEnumerable<Gate> InputGates
        {
            get
            {
                yield return _inputGate;
            }
        }

        public Gate Input
        {
            get
            {
                return _inputGate;
            }
        }
    }
}
