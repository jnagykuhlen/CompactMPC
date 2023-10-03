using System;

namespace CompactMPC.Circuits.Batching.Internal
{
    public sealed class ForwardInputGate : ForwardGate
    {
        protected override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            throw new InvalidOperationException("Input gate cannot receive input values.");
        }

        protected override void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            throw new InvalidOperationException("Input gate cannot receive visiting requests.");
        }
        
        public override bool IsAssignable => true;
    }
}
