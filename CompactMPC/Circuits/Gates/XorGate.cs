using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Circuits.Gates
{
    public class XorGate : Gate
    {
        private Gate _leftInputGate;
        private Gate _rightInputGate;

        public XorGate(GateContext context, Gate leftInputGate, Gate rightInputGate)
             : base(context)
        {
            _leftInputGate = leftInputGate;
            _rightInputGate = rightInputGate;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            CircuitEvaluationState<T> evaluationState,
            CircuitContext circuitContext)
        {
            T value = evaluator.EvaluateXorGate(
                evaluationState.GetGateEvaluationValue(_leftInputGate),
                evaluationState.GetGateEvaluationValue(_rightInputGate),
                Context,
                circuitContext
            );

            evaluationState.SetGateEvaluationValue(this, value);
        }

        public override IEnumerable<Gate> InputGates
        {
            get
            {
                yield return _leftInputGate;
                yield return _rightInputGate;
            }
        }

        public Gate LeftInputGate
        {
            get
            {
                return _leftInputGate;
            }
        }

        public Gate RightInputGate
        {
            get
            {
                return _rightInputGate;
            }
        }
    }
}
