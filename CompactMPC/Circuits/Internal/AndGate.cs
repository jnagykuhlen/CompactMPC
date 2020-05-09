using System.Collections.Generic;

namespace CompactMPC.Circuits.Internal
{
    public class AndGate : Gate
    {
        private readonly Gate _leftInputGate;
        private readonly Gate _rightInputGate;

        public AndGate(int id, Gate leftInputGate, Gate rightInputGate)
             : base(id)
        {
            _leftInputGate = leftInputGate;
            _rightInputGate = rightInputGate;
        }
        
        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            T value = evaluator.EvaluateAndGate(
                evaluationState.GetGateEvaluationValue(_leftInputGate),
                evaluationState.GetGateEvaluationValue(_rightInputGate)
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
    }
}
