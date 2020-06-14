namespace CompactMPC.Circuits.Batching.Internal
{
    public class ForwardXorGate : BinaryForwardGate
    {
        public ForwardXorGate(ForwardGate leftInputGate, ForwardGate rightInputGate)
        {
            AddPredecessor(leftInputGate);
            AddPredecessor(rightInputGate);
        }
        
        protected override void ReceiveInputValues<T>(T leftValue, T rightValue, IBatchCircuitEvaluator<T> evaluator, ForwardEvaluationState<T> evaluationState)
        {
            T outputValue = evaluator.EvaluateXorGate(leftValue, rightValue);
            SendOutputValue(outputValue, evaluator, evaluationState);
        }

        protected override void Visit(ICircuitVisitor visitor)
        {
            visitor.VisitXorGate();
        }
    }
}
