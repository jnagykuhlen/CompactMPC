namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardAndGate : BinaryForwardGate
    {
        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            GateEvaluationInput<T> evaluationInput = new GateEvaluationInput<T>(leftValue, rightValue);
            evaluationState.DelayAndGateEvaluation(new GateEvaluation<T>(this, evaluationInput));
        }
    }
}
