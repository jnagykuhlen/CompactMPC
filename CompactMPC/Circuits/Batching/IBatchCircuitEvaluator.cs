namespace CompactMPC.Circuits.Batching
{
    public interface IBatchCircuitEvaluator<T>
    {
        T[] EvaluateAndGateBatch(GateEvaluationInput<T>[] evaluationInputs);
        T EvaluateXorGate(T leftValue, T rightValue);
        T EvaluateNotGate(T value);
    }
}
