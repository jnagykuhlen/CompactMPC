using System.Collections.Generic;

namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class ForwardGate
    {
        private readonly List<ForwardGate> _successors;

        protected ForwardGate()
        {
            _successors = new List<ForwardGate>();
        }

        public abstract void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
        public abstract void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState);

        public void SendOutputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            foreach (ForwardGate successor in _successors)
                successor.ReceiveInputValue(value, evaluator, evaluationState);
        }
        
        public void SendVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            foreach (ForwardGate successor in _successors)
                successor.ReceiveVisitingRequest(visitor, visitingState);
        }

        public void AddSuccessor(ForwardGate successor)
        {
            _successors.Add(successor);
        }
    }
}
