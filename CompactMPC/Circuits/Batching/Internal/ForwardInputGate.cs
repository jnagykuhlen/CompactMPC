namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardInputGate : UnaryForwardGate
    {
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            SendOutputValue(value, evaluator, evaluationState);
        }

        protected override void Visit(ICircuitVisitor visitor)
        {
            visitor.VisitInputGate();
        }
    }
}
