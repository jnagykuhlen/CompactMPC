namespace CompactMPC.Circuits.Batching
{
    public class BatchCircuitEvaluator<T> : IBatchCircuitEvaluator<T>
    {
        private readonly ICircuitEvaluator<T> _innerEvaluator;

        public BatchCircuitEvaluator(ICircuitEvaluator<T> innerEvaluator)
        {
            _innerEvaluator = innerEvaluator;
        }
        
        public T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs)
        {
            T[] outputValues = new T[evaluationInputs.Length];
            for (int i = 0; i < evaluationInputs.Length; ++i)
            {
                GateEvaluationInput<T> evaluationInput = evaluationInputs[i];
                outputValues[i] = _innerEvaluator.EvaluateAndGate(
                    evaluationInput.LeftValue,
                    evaluationInput.RightValue
                );
            }

            return outputValues;
        }

        public T EvaluateXorGate(T leftValue, T rightValue)
        {
            return _innerEvaluator.EvaluateXorGate(leftValue, rightValue);
        }

        public T EvaluateNotGate(T value)
        {
            return _innerEvaluator.EvaluateNotGate(value);
        }
    }
}
