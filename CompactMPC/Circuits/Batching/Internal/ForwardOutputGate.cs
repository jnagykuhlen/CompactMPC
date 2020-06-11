namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardOutputGate : UnaryForwardGate
    {
        private readonly int _outputIndex;

        // TODO: Only for compatibility with ForwardCircuitBuilder
        public ForwardOutputGate(int outputIndex)
        {
            _outputIndex = outputIndex;
        }
        
        public ForwardOutputGate(ForwardGate inputGate, int outputIndex)
        {
            _outputIndex = outputIndex;
            inputGate.AddSuccessor(this);
        }
        
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            evaluationState.SetOutput(_outputIndex, value);
        }

        protected override void Visit(ICircuitVisitor visitor)
        {
            visitor.VisitOutputGate();
        }
    }
}
