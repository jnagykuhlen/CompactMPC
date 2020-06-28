namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardNotGate : UnaryForwardGate
    {
        public ForwardNotGate(ForwardGate inputGate)
        {
            AddPredecessor(inputGate);
        }

        protected override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            T outputValue = evaluator.EvaluateNotGate(value);
            SendOutputValue(outputValue, evaluator, evaluationState);
        }

        protected override void Visit(ICircuitVisitor visitor)
        {
            visitor.VisitNotGate();
        }
    }
}
