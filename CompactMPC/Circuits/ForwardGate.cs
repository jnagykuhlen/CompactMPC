using System.Collections.Generic;
using CompactMPC.Circuits.Internal;

namespace CompactMPC.Circuits;

public abstract class ForwardGate
{
    private readonly List<ForwardGate> _successors = new();

    public void SendOutputValue<T>(T value, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
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

    protected void AddPredecessor(ForwardGate predecessor) => predecessor._successors.Add(this);

    protected abstract void ReceiveInputValue<T>(T value, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState);
    protected abstract void ReceiveVisitingRequest(ICircuitVisitor visitor, ForwardVisitingState visitingState);
    public virtual bool IsAssignable => false;
}
