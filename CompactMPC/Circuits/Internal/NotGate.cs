using System.Collections.Generic;

namespace CompactMPC.Circuits.Internal
{
    public class NotGate : Gate
    {
        private readonly Gate _inputGate;

        public NotGate(int id, Gate inputGate)
             : base(id)
        {
            _inputGate = inputGate;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            T value = evaluator.EvaluateNotGate(
                evaluationState.GetGateEvaluationValue(_inputGate)
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
    }
}
