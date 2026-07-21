namespace CompactMPC.Circuits.Internal;

public class ForwardAndGate : BinaryForwardGate
{
    public ForwardAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
    {
        AddPredecessor(leftInputGate);
        AddPredecessor(rightInputGate);
    }
        
    protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IAsyncBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
    {
        var evaluationInput = new GateEvaluationInput<T>(leftValue, rightValue);
        evaluationState.DelayAndGateEvaluation(new GateEvaluation<T>(this, evaluationInput));
    }
}
