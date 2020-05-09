using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class ForwardGate
    {
        private readonly List<ForwardGate> _successors;

        public ForwardGate()
        {
            _successors = new List<ForwardGate>();
        }

        public abstract void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);

        public void SendOutputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            foreach (ForwardGate successor in _successors)
                successor.ReceiveInputValue(value, evaluator, evaluationState);
        }

        public void AddSuccessor(ForwardGate sucessor)
        {
            _successors.Add(sucessor);
        }
    }
}
