namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardXorGate : BinaryForwardGate
    {
        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            T outputValue = evaluator.EvaluateXorGate(leftValue, rightValue);
            SendOutputValue(outputValue, evaluator, evaluationState);
        }
    }
}
