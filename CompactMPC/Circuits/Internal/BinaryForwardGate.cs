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

    protected abstract void ReceiveInputValues<T>(T leftValue, T rightValue, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
}
