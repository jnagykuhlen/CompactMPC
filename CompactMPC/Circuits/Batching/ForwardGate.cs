using System.Collections.Generic;
using CompactMPC.Circuits.Batching.Internal;

namespace CompactMPC.Circuits.Batching
{
    public abstract class ForwardGate
    {
        private readonly List<ForwardGate> _successors = new List<ForwardGate>();
        
        public abstract void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
        public abstract void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState);

        public void SendOutputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            evaluationState.SetOutputValue(this, value);
            foreach (ForwardGate successor in _successors)
                successor.ReceiveInputValue(value, evaluator, evaluationState);
        }
        
        public void SendVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            foreach (ForwardGate successor in _successors)
                successor.ReceiveVisitingRequest(visitor, visitingState);
        }

        protected void AddPredecessor(ForwardGate predecessor)
        {
            predecessor._successors.Add(this);
        }
    }
}
