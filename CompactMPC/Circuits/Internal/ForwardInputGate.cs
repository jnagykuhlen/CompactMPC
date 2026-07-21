using System;

namespace CompactMPC.Circuits.Internal;

public sealed class ForwardInputGate : ForwardGate
{
    protected override void ReceiveInputValue<T>(T value, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState) =>
        throw new InvalidOperationException("Input gate cannot receive input values.");

    public override bool IsAssignable => true;
}
