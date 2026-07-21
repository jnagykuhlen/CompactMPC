namespace CompactMPC.Circuits.Internal;

public abstract class BinaryForwardGate : ForwardGate
{
    protected sealed override void ReceiveInputValue<T>(T value, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
    {
        if (evaluationState.ReadInputValueFromCache(this, out var cachedInputValue))
            ReceiveInputValues(cachedInputValue, value, evaluator, evaluationState);
        else
            evaluationState.WriteInputValueToCache(this, value);
    }

    protected sealed override void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState)
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

    protected abstract void ReceiveInputValues<T>(T leftValue, T rightValue, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
    protected abstract void Visit(ICircuitVisitor visitor);
}
