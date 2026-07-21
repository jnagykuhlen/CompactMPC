namespace CompactMPC.Circuits.Internal;

public class ForwardXorGate : BinaryForwardGate
{
    public ForwardXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
    {
        AddPredecessor(leftInputGate);
        AddPredecessor(rightInputGate);
    }
        
    protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
    {
        var outputValue = evaluator.EvaluateXorGate(leftValue, rightValue);
        SendOutputValue(outputValue, evaluator, evaluationState);
    }
}
