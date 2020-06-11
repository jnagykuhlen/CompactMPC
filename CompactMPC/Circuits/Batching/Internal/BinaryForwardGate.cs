﻿namespace CompactMPC.Circuits.Batching.Internal
{
    public abstract class BinaryForwardGate : ForwardGate
    {
        public sealed override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            Optional<T> cachedInputValue = evaluationState.ReadInputValueFromCache(this);
            if (cachedInputValue.IsPresent)
                ReceiveInputValues(cachedInputValue.Value, value, evaluator, evaluationState);
            else
                evaluationState.WriteInputValueToCache(this, value);
        }

        public sealed override void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
        {
            if (visitingState.HasVisitingRequest(this))
            {
                visitingState.RemoveVisitingRequest(this);
                Visit(visitor);
                SendVisitingRequest(visitor, visitingState);
            }
            else
            {
                visitingState.AddVisitingRequest(this);
            }
        }

        protected abstract void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
        protected abstract void Visit(ICircuitVisitor visitor);
    }
}
