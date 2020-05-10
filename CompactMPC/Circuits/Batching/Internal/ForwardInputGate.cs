namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardInputGate : ForwardGate
    {
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            SendOutputValue(value, evaluator, evaluationState);
        }
    }
}
