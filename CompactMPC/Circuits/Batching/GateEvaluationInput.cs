namespace CompactMPC.Circuits.Batching
{
    public class GateEvaluationInput<T>
    {
        public T LeftValue { get; }
        public T RightValue { get; }
        
        public GateEvaluationInput(T leftValue, T rightValue)
        {
            LeftValue = leftValue;
            RightValue = rightValue;
        }

        
    }
}
