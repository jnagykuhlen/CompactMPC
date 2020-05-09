using System.Collections.Generic;

namespace CompactMPC.Circuits.Internal
{
    public class OutputGate : Gate
    {
        private readonly Gate _inputGate;
        private readonly int _outputIndex;

        public OutputGate(int id, Gate inputGate, int outputIndex)
             : base(id)
        {
            _inputGate = inputGate;
            _outputIndex = outputIndex;
        }

        public override void Evaluate<T>(
            ICircuitEvaluator<T> evaluator,
            EvaluationState<T> evaluationState)
        {
            evaluationState.SetOutput(_outputIndex, evaluationState.GetGateEvaluationValue(_inputGate));
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
