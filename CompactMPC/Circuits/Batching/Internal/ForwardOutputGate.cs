namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardOutputGate : ForwardGate
    {
        private readonly int _outputIndex;

        public ForwardOutputGate(int outputIndex)
        {
            _outputIndex = outputIndex;
        }
        
        public override void ReceiveInputValue<T>(T value, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            evaluationState.SetOutput(_outputIndex, value);
        }
    }
}
