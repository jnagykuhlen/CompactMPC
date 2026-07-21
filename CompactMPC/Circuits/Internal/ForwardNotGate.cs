namespace CompactMPC.Circuits.Internal;

public class ForwardNotGate : ForwardGate
{
    public ForwardNotGate(ForwardGate inputGate) => AddPredecessor(inputGate);

    protected override void ReceiveInputValue<T>(T value, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
    {
        var outputValue = evaluator.EvaluateNotGate(value);
        SendOutputValue(outputValue, evaluator, evaluationState);
    }
}
