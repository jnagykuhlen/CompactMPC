using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Internal
{
    public class InputGate : Gate
    {
        private int _inputIndex;

        public InputGate(int id, int inputIndex)
             : base(id)
        {
            _inputIndex = inputIndex;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            evaluationState.SetGateEvaluationValue(this, evaluationState.GetInput(_inputIndex));
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
