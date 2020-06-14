namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardAndGate : BinaryForwardGate
    {
        public ForwardAndGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            AddPredecessor(leftInputGate);
            AddPredecessor(rightInputGate);
        }
        
        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            GateEvaluationInput<T> evaluationInput = new GateEvaluationInput<T>(leftValue, rightValue);
            evaluationState.DelayAndGateEvaluation(new GateEvaluation<T>(this, evaluationInput));
        }

        protected override void Visit(ICircuitVisitor visitor)
        {
            visitor.VisitAndGate();
        }
    }
}
